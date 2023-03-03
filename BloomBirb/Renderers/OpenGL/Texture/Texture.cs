using System.Diagnostics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class Texture : ITexture
{
    public uint TextureHandle { get; private set; }

    private readonly OpenGLRenderer renderer;

    public bool HasTransparency { get; private set; }

    public Size TextureSize { get; private set; }

    protected int MipMapLevels { get; private set; }

    public Texture(OpenGLRenderer renderer, int mipLevels = 4)
    {
        this.renderer = renderer;
        MipMapLevels = mipLevels;
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
        if (rect.Size.X * rect.Size.Y <= 0)
            return;

        Span<Rgba32> pixels = new Rgba32[rect.Size.X * rect.Size.Y];

        pixels.Fill(pixel);

        unsafe
        {
            fixed (void* data = pixels)
                renderer.Context?.TexSubImage2D(TextureTarget.Texture2D, 0, rect.Origin.X, rect.Origin.Y, (uint)rect.Size.X, (uint)rect.Size.Y, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }
    }

    public unsafe void BufferImageData(Image<Rgba32> image, int offsetX = 0, int offsetY = 0, int padding = 0)
    {
        Bind();
        image.ProcessPixelRows(accessor =>
        {
            int paddedOffsetX = Math.Max(offsetX - padding, 0);
            int paddedOffsetY = Math.Max(offsetY - padding, 0);
            int paddedWidth = image.Width + (offsetX - paddedOffsetX) + Math.Clamp(TextureSize.Width - (offsetX + image.Width), 0, padding);
            int paddedHeight = image.Height + (offsetY - paddedOffsetY) + Math.Clamp(TextureSize.Height - (offsetY + image.Height), 0, padding);

            Span<Rgba32> paddedRow = stackalloc Rgba32[paddedWidth];

            for (int i = 0; i < paddedHeight; i++)
            {
                int rowIndex = Math.Clamp(paddedOffsetY - offsetY + i, 0, accessor.Height - 1);
                var rowSpan = accessor.GetRowSpan(rowIndex);

                if (!HasTransparency)
                    foreach (var pix in rowSpan)
                        if (pix.A < 255 && pix.A > 25)
                        {
                            HasTransparency = true;
                            break;
                        }

                for (int j = 0; j < paddedWidth; ++j)
                {
                    int colIndex = Math.Clamp(paddedOffsetX - offsetX + j, 0, accessor.Width - 1);
                    paddedRow[j] = rowSpan[colIndex];
                }

                fixed (void* data = paddedRow)
                    renderer.Context?.TexSubImage2D(TextureTarget.Texture2D, 0, paddedOffsetX, paddedOffsetY + i, (uint)paddedWidth, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
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
        renderer.Context?.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, MipMapLevels);
    }

    private void generateMipmap() => renderer.Context?.GenerateMipmap(TextureTarget.Texture2D);

    public void Bind() => renderer.BindTexture(this);

    public static implicit operator TextureUsage(Texture texture) => texture.AsTextureUsage();

    public TextureUsage AsTextureUsage() => new(this, new(0, 0, TextureSize.Width, TextureSize.Height), HasTransparency);
}
