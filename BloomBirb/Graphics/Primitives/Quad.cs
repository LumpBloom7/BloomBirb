using System.Numerics;
using System.Runtime.InteropServices;
using BloomBirb.Extensions;

namespace BloomBirb.Graphics.Primitives;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Quad
{
    public static readonly Quad DEFAULT = new(-0.5f, -0.5f, 1, 1);

    public readonly Vector2 TopLeft;
    public readonly Vector2 BottomLeft;
    public readonly Vector2 BottomRight;
    public readonly Vector2 TopRight;

    public Quad(Vector2 topLeft, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight)
    {
        TopLeft = topLeft;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
        TopRight = topRight;
    }

    public Quad(float x, float y, float width, float height)
    {
        TopLeft = new Vector2(x, y);
        BottomLeft = new Vector2(x, y + height);
        BottomRight = new Vector2(x + width, y + height);
        TopRight = new Vector2(x + width, y);
    }

    public static Quad operator *(Quad quad, Matrix3 transformation) => new(
        quad.TopLeft.Transform(transformation),
        quad.BottomLeft.Transform(transformation),
        quad.BottomRight.Transform(transformation),
        quad.TopRight.Transform(transformation)
    );
}
