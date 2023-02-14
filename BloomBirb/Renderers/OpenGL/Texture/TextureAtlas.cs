using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class TextureAtlas : Texture
{
    public TextureAtlas(OpenGLRenderer renderer)
        : base(renderer) { }

    public override void Initialize(Size size)
    {
        base.Initialize(size);
        rectangles = new List<Rectangle<int>>
        {
            new (0,0,size.Width, size.Height)
        };
    }

    public unsafe TextureUsage? AddSubtexture(Image<Rgba32> image)
    {
        int width = image.Width, height = image.Height;

        if (!tryGetRegion(width, height, out var rect))
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

        BufferImageData(image, offsetX, offsetY);

        var floatRect = new System.Drawing.RectangleF(offsetX / (float)TextureSize.Width,
                                                              offsetY / (float)TextureSize.Height,
                                                              width / (float)TextureSize.Width,
                                                              height / (float)TextureSize.Height);

        return new TextureUsage(this, floatRect, transparent);

    }

    private List<Rectangle<int>> rectangles = null!;

    private bool tryGetRegion(int desiredSizeX, int desiredSizeY, out Rectangle<int>? rectangle)
    {
        // Get all the rectangles which can fit our new texture, sorted by area
        var sizeSortedRects = rectangles.Where(r => r.Size.X >= desiredSizeX && r.Size.Y >= desiredSizeY)
                                                          .OrderBy(r => r.Size.X * r.Size.Y).ToArray();

        if (sizeSortedRects.Length == 0)
        {
            rectangle = null;
            return false;
        }

        var smallestFittingRect = sizeSortedRects.First();

        // Create a sub rectangle from the chosen rect
        rectangle = new Rectangle<int>(smallestFittingRect.Origin, desiredSizeX, desiredSizeY);

        // For any rectangle, after taking out a sub rectangle, the remaining space can be represented with two rectangles
        // So we remove the original big rectangle from our list, and put in the two new rectangles

        var rect1 = new Rectangle<int>(smallestFittingRect.Origin.X + desiredSizeX,
                                                      smallestFittingRect.Origin.Y,
                                                      smallestFittingRect.Size.X - desiredSizeX,
                                                      smallestFittingRect.Size.Y);

        var rect2 = new Rectangle<int>(smallestFittingRect.Origin.X, smallestFittingRect.Origin.Y + desiredSizeY, smallestFittingRect.Size.X, smallestFittingRect.Size.Y + desiredSizeY);

        rectangles.Remove(smallestFittingRect);
        rectangles.Add(rect1);
        rectangles.Add(rect2);

        return true;
    }
}
