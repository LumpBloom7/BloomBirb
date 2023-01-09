using System.Numerics;

namespace BloomBirb.Extensions;

public static class Matrix3Extensions
{
    public static void RotateDegrees(ref Matrix3 m, float degrees) => RotateRadians(ref m, degrees * (float.Pi / 180));

    public static void RotateRadians(ref Matrix3 m, float radians)
    {
        (float sin, float cos) = MathF.SinCos(radians);

        var r1 = (m.Row1 * cos) + (m.Row2 * sin);
        m.Row2 = (m.Row2 * cos) - (m.Row1 * sin);

        m.Row1 = r1;
    }

    public static void Shear(ref Matrix3 m, Vector2 vec)
        => Shear(ref m, vec.X, vec.Y);

    public static void Shear(ref Matrix3 m, float x, float y)
    {
        var r1 = m.Row1 + m.Row2 * y;
        m.Row2 += m.Row1 * x;
        m.Row1 = r1;
    }

    public static void Scale(ref Matrix3 m, Vector2 vec)
        => Scale(ref m, vec.X, vec.Y);

    public static void Scale(ref Matrix3 m, float x, float y)
    {
        m.Row1 *= x;
        m.Row2 *= y;
    }

    public static void Translate(ref Matrix3 m, Vector2 vec)
        => Translate(ref m, vec.X, vec.Y);

    public static void Translate(ref Matrix3 m, float x, float y)
    {
        m.M31 += x;
        m.M32 += y;
    }
}
