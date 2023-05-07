using System.Diagnostics.CodeAnalysis;
using BloomFramework.Graphics.Textures;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class TextureAtlas : Texture
{
    public TextureAtlas(OpenGlRenderer renderer, FilterMode filterMode, int mipLevels)
        : base(renderer, filterMode, mipLevels) { }

    public override void Initialize(Size size)
    {
        base.Initialize(size);
        int halfPad = paddingAmount / 2;
        for (int i = 0; i < halfPad + 1; ++i)
            for (int j = 0; j < halfPad + 1; ++j)
                SetPixel(i, j, new Rgba32(255, 255, 255));

        currentCoord = new Vector2D<int>(1 + paddingAmount, 0);
        maxY = 1 + paddingAmount;
    }

    public TextureUsage? AddSubtexture(Image<Rgba32> image)
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
                {
                    if (pix.A < 255)
                    {
                        transparent = true;
                        break;
                    }
                }
            }
        });

        BufferImageData(image, offsetX, offsetY, paddingAmount / 2);

        return new TextureUsage(this, rect.Value, transparent);
    }

    private Vector2D<int> currentCoord = Vector2D<int>.Zero;
    private int maxY;


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
    private int paddingAmount => 2 << MipMapLevels;
}