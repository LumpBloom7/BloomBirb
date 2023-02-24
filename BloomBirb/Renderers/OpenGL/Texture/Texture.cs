using System.Diagnostics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class Texture
{
    protected uint TextureHandle { get; private set; }

    private readonly OpenGLRenderer renderer;

    public bool HasTransparency { get; private set; }

    protected Size TextureSize { get; private set; }

    public Texture(OpenGLRenderer renderer)
    {
        this.renderer = renderer;
    }

    public virtual unsafe void Initialize(Size size)
    {
        Debug.Assert(renderer.Context is not null);

        TextureSize = size;

        TextureHandle = renderer.Context.GenTexture();

        Bind();

        renderer.Context.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)size.Width, (uint)size.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, (void*)null);

        setParameters();
    }

    public void SetPixel(int offsetX, int offsetY, Rgba32 pixel)
    {
        renderer.Context?.TexSubImage2D(TextureTarget.Texture2D, 0, offsetX, offsetY, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, in pixel);
    }

    protected void PaintRectangle(Rectangle<int> rect, Rgba32 pixel)
    {
        Span<Rgba32> pixels = new Rgba32[rect.Size.X * rect.Size.Y];

        pixels.Fill(pixel);

        unsafe
        {
            fixed (void* data = pixels)
                renderer.Context?.TexSubImage2D(TextureTarget.Texture2D, 0, rect.Origin.X, rect.Origin.Y, (uint)rect.Size.X, (uint)rect.Size.Y, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }
    }
    public unsafe void BufferImageData(Image<Rgba32> image, int offsetX = 0, int offsetY = 0)
    {
        Bind();
        image.ProcessPixelRows(accessor =>
        {
            for (int i = 0; i < accessor.Height; i++)
            {
                var rowSpan = accessor.GetRowSpan(i);

                if (!HasTransparency)
                    foreach (var pix in rowSpan)
                        if (pix.A < 255)
                        {
                            HasTransparency = true;
                            break;
                        }

                fixed (void* data = rowSpan)
                    renderer.Context?.TexSubImage2D(TextureTarget.Texture2D, 0, offsetX, offsetY + i, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }
        });

        generateMipmap();
    }

    private void setParameters()
    {
        // Set some parameters
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)GLEnum.LinearMipmapLinear);
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
    }

    private void generateMipmap() => renderer.Context?.GenerateMipmap(TextureTarget.Texture2D);

    public void Bind() => renderer.BindTexture(TextureHandle);

    public static implicit operator TextureUsage(Texture texture) => texture.AsTextureUsage();

    public TextureUsage AsTextureUsage() => new(this, new System.Drawing.RectangleF(0, 0, 1, 1), HasTransparency);
}
