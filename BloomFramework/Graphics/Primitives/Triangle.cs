using System.Numerics;

namespace BloomFramework.Graphics.Primitives;

public readonly record struct Triangle
{
    public readonly Vector2 A;
    public readonly Vector2 B;
    public readonly Vector2 C;

    public readonly float Area;

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
        Area = computeArea(a, b, c);
    }

    public bool Contains(Vector2 point)
    {
        float area1 = computeArea(point, A, B);
        float area2 = computeArea(point, B, C);
        float area3 = computeArea(point, A, C);

        return Math.Abs(area1 + area2 + area3 - Area) < float.Epsilon;
    }

    private static float computeArea( Vector2 a, Vector2 b, Vector2 c)
    {
        return Math.Abs(0.5f * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)));
    }
}
