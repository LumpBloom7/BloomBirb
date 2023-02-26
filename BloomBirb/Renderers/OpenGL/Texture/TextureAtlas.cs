using System.Diagnostics.CodeAnalysis;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class TextureAtlas : Texture
{
    public TextureAtlas(OpenGLRenderer renderer, int mipLevels)
        : base(renderer) { }

    public unsafe TextureUsage? AddSubtexture(Image<Rgba32> image)
    {
        int width = image.Width, height = image.Height;

        if (!findFittingRect(width, height, out var rect))
            return null;

        int offsetX = rect.Value.Origin.X;
        int offsetY = rect.Value.Origin.Y;

        bool transparent = false;

        image.ProcessPixelRows(accessor =>
        {
            for (int i = 0; i < accessor.Height; i++)
            {
                foreach (var pix in accessor.GetRowSpan(i))
                    if (pix.A < 255)
                    {
                        transparent = true;
                        break;
                    }
            }
        });

        BufferImageData(image, offsetX, offsetY, paddingAmount / 2);

        var floatRect = new System.Drawing.RectangleF(offsetX / (float)TextureSize.Width,
                                                              offsetY / (float)TextureSize.Height,
                                                              width / (float)TextureSize.Width,
                                                              height / (float)TextureSize.Height);

        return new TextureUsage(this, floatRect, transparent);

    }

    private Vector2D<int> currentCoord = Vector2D<int>.Zero;
    private int maxY = 0;

    private bool findFittingRect(int desiredSizeX, int desiredSizeY, [NotNullWhen(true)] out Rectangle<int>? rectangle)
    {
        var nextCoord = currentCoord;
        int newMaxY = maxY;

        if (desiredSizeX > TextureSize.Width - nextCoord.X)
        {
            nextCoord = new Vector2D<int>(0, nextCoord.Y + maxY);
            newMaxY = 0;
        }

        if ((desiredSizeY > TextureSize.Height - nextCoord.Y) || (desiredSizeX > TextureSize.Width))
        {
            rectangle = null;
            return false;
        }

        rectangle = new(nextCoord, desiredSizeX, desiredSizeY);
        maxY = Math.Max(newMaxY, desiredSizeY + paddingAmount);
        currentCoord = new(nextCoord.X + desiredSizeX + paddingAmount, nextCoord.Y);
        return true;
    }

    // This is the amount of padding between two textures
    private int paddingAmount => (int)Math.Pow(2, MipMapLevels);
}
