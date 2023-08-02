using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class Texture : ITexture, IDisposable
{
    protected readonly OpenGlRenderer Renderer;

    public uint TextureHandle { get; private set; }

    protected readonly int Width, Height;

    public Vector2D<int> TextureSize => new(Width, Height);

    public bool HasTransparencies => true;

    protected readonly TextureParameters TextureParameters;

    private bool invalidated = false;
    protected int MipmapLevels;

    public Texture(OpenGlRenderer renderer, int width, int height)
        : this(renderer, width, height, new TextureParameters())
    {
    }

    public Texture(OpenGlRenderer renderer, int width, int height,
        TextureParameters parameters)
    {
        Renderer = renderer;
        TextureParameters = parameters;
        (Width, Height) = (width, height);

        // Expectation, GL is ready at this point
        TextureHandle = Renderer.Context.GenTexture();

        Bind();

        MipmapLevels = Math.Min(parameters.MaxMipLevel, ((int)MathF.Log2(Math.Max(width, height))) + 1);
        unsafe
        {
            Renderer.Context.TexStorage2D(TextureTarget.Texture2D, (uint)MipmapLevels, SizedInternalFormat.Rgba8, (uint)Width, (uint)Height);
        }

        setTextureParameters();

        invalidated = true;
    }

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        Renderer.BindTexture(this, textureUnit);

        // Regenerate mipmaps if necessary
        if (!invalidated) return;

        generateMipmaps();
        invalidated = false;
    }

    private void setTextureParameters()
    {
        Renderer.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureParameters.TextureWrapX);
        Renderer.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureParameters.TextureWrapY);

        Renderer.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureParameters.TextureMinFilter);
        Renderer.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureParameters.TextureMagFilter);

        Renderer.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel,
            TextureParameters.BaseMipLevel);
        Renderer.Context.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel,
            TextureParameters.MaxMipLevel);
    }

    public ITexture UploadData(Image<Rgba32> image, int padding = 0) => UploadData(image, new Vector2D<int>(), padding);

    public unsafe ITexture UploadData(Image<Rgba32> image, Vector2D<int> targetOffset, int padding = 0)
    {
        int targetWidth = Math.Min(image.Width, Width - targetOffset.X);
        int targetHeight = Math.Min(image.Height, Height - targetOffset.Y);
        bool hasTransparencies = false;

        int leftSpace = Math.Clamp(targetOffset.X, 0, padding);
        int rightSpace = Math.Clamp(Width - (targetOffset.X + targetWidth), 0, padding);

        image.ProcessPixelRows(p =>
        {
            Span<Rgba32> paddedRowSpan = stackalloc Rgba32[leftSpace + targetWidth + rightSpace];

            for (int i = -padding; i < targetHeight + padding; ++i)
            {
                if ((targetOffset.Y + i < 0) || (targetOffset.Y + i >= Height))
                    continue;

                var rowSpan = p.GetRowSpan(Math.Clamp(i, 0, image.Height - 1))[..targetWidth];

                invalidated = invalidated || rowSpan.Length > 0;

                hasTransparencies = hasTransparencies || hasTransparentPixels(rowSpan);

                padPixelRow(rowSpan, paddedRowSpan, leftSpace);

                fixed (void* data = paddedRowSpan)
                    Renderer.Context.TexSubImage2D(TextureTarget.Texture2D, 0, targetOffset.X - leftSpace,
                        i + targetOffset.Y,
                        (uint)paddedRowSpan.Length, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }
        });

        return new TextureRegion(this, new Rectangle<int>(targetOffset, targetWidth, targetHeight), hasTransparencies);
    }

    private void generateMipmaps() => Renderer.Context.GenerateMipmap(TextureTarget.Texture2D);

    private static void padPixelRow(Span<Rgba32> original, Span<Rgba32> outputSpan, int leftPadding)
    {
        for (int i = 0; i < outputSpan.Length; ++i)
            outputSpan[i] = original[Math.Clamp(i - leftPadding, 0, original.Length - 1)];
    }

    private static bool hasTransparentPixels<T>(Span<T> pixelSpan) where T : unmanaged, IPixel<T>
    {
        Rgba32 tmp = new Rgba32();
        foreach (var pixel in pixelSpan)
        {
            pixel.ToRgba32(ref tmp);
            if (tmp.A < 255)
                return true;
        }

        return false;
    }

    public void Dispose()
    {
        Renderer.Context.DeleteTexture(TextureHandle);
        TextureHandle = 0;

        GC.SuppressFinalize(this);
    }

    ~Texture() => Dispose();
}
