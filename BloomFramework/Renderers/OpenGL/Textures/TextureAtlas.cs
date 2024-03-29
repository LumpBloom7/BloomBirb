using System.Diagnostics.CodeAnalysis;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class TextureAtlas : Texture
{
    public readonly TextureRegion WhitePixelRegion;

    public TextureAtlas(OpenGlRenderer renderer, int width, int height)
        : this(renderer, width, height, new TextureParameters())
    {

    }

    public TextureAtlas(OpenGlRenderer renderer, int width, int height, TextureParameters parameters)
        : base(renderer, width, height, parameters)
    {
        // Prepare a white dot for use in blank textures
        createWhiteDot();

        WhitePixelRegion = new TextureRegion(this, new Rectangle<int>(0, 0, 0, 0), false);
    }

    private unsafe void createWhiteDot()
    {
        int width = 1 + paddingAmount;
        Span<Rgba32> whitePixels = stackalloc Rgba32[width * width];

        whitePixels.Fill(new Rgba32(1f, 1f, 1f));

        fixed (void* data = whitePixels)
            Renderer.Context.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)width, (uint)width,
                PixelFormat.Rgba, PixelType.UnsignedByte, data);

        cursorX = nextRowY = 1 + paddingAmount * 2;
    }

    public bool TryAddSubtexture(Image<Rgba32> image, [NotNullWhen(true)] out ITexture? textureRegion)
    {
        var target = findFittingPosition(image.Width, image.Height);
        if (target is not { } offset)
        {
            textureRegion = null;
            return false;
        }

        textureRegion = UploadData(image, offset, paddingAmount);
        return true;
    }

    private int paddingAmount => 1 << MipmapLevels;

    private int cursorX, cursorY;
    private int nextRowY;

    private Vector2D<int>? findFittingPosition(int width, int height)
    {
        // This definitely can't fit in the atlas
        if (width > Width || height > Height)
            return null;

        // Not enough vertical space
        if (cursorY + height > Height)
            return null;

        // Can't possibly find a position to fit rect
        if (cursorX + width > Width && nextRowY + height > Height)
            return null;

        if (cursorX + width > Width)
            (cursorX, cursorY) = (0, nextRowY);

        var result = new Vector2D<int>(cursorX, cursorY);

        cursorX += width + paddingAmount * 2;
        nextRowY = Math.Max(nextRowY, cursorY + height + paddingAmount * 2);

        return result;
    }
}
