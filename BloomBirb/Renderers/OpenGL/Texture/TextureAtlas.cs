using System.Diagnostics.CodeAnalysis;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class TextureAtlas : Texture
{
    public TextureAtlas(OpenGLRenderer renderer, int mipLevels)
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

        BufferImageData(image, offsetX, offsetY, paddingAmount / 2);

        var floatRect = new System.Drawing.RectangleF(offsetX / (float)TextureSize.Width,
                                                              offsetY / (float)TextureSize.Height,
                                                              width / (float)TextureSize.Width,
                                                              height / (float)TextureSize.Height);

        return new TextureUsage(this, floatRect, transparent);

    }



    private List<Rectangle<int>> rectangles = null!;

    // This is the amount of padding between two textures
    private int paddingAmount => (int)Math.Pow(2, MipMapLevels);

    private bool tryGetRegion(int desiredSizeX, int desiredSizeY, [NotNullWhen(true)] out Rectangle<int>? rectangle)
    {
        // Get all the rectangles which can fit our new texture, sorted by area
        var sizeSortedRects = rectangles.Where(r => r.Size.X >= desiredSizeX && r.Size.Y >= desiredSizeY)
                                                          .OrderBy(r => r.Size.X * r.Size.Y).ToArray();

        int padding = paddingAmount;

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

        var rect1 = new Rectangle<int>(smallestFittingRect.Origin.X + desiredSizeX + padding,
                                                      smallestFittingRect.Origin.Y,
                                                      smallestFittingRect.Size.X - desiredSizeX - padding,
                                                      desiredSizeY);

        var rect2 = new Rectangle<int>(smallestFittingRect.Origin.X,
                                                      smallestFittingRect.Origin.Y + desiredSizeY + padding,
                                                      smallestFittingRect.Size.X,
                                                      smallestFittingRect.Size.Y - desiredSizeY - padding);

        var rect1_ = new SixLabors.ImageSharp.Rectangle(rect1.Origin.X, rect1.Origin.Y, rect1.Size.X, rect1.Size.Y);
        var rect2_ = new SixLabors.ImageSharp.Rectangle(rect2.Origin.X, rect2.Origin.Y, rect2.Size.X, rect2.Size.Y);

        var intersect = SixLabors.ImageSharp.Rectangle.Intersect(rect1_, rect2_);

        // For debugging purposes
        PaintRectangle(rect1, new Rgba32(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle(), 1));
        PaintRectangle(rect2, new Rgba32(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle(), 1));
        PaintRectangle(new(intersect.X, intersect.Y, intersect.Width, intersect.Height), new Rgba32(0, 0, 0, 0));
        // Debugging ends here

        rectangles.Remove(smallestFittingRect);

        if (rect1.Size.X * rect1.Size.Y > 0)
            rectangles.Add(rect1);

        if (rect2.Size.X * rect2.Size.Y > 0)
            rectangles.Add(rect2);

        return true;
    }
}
