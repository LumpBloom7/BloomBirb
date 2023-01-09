using System.Numerics;

namespace BloomBirb.Extensions;

public static class Matrix4Extensions
{
    public static Matrix4x4 RotateDegrees(this Matrix4x4 matrix, float degrees)
        => matrix.RotateRadians(degrees * (float.Pi / 180));

    public static Matrix4x4 RotateRadians(this Matrix4x4 matrix, float radians)
    {
        (float sin, float cos) = MathF.SinCos(radians);

        var t = new Matrix4x4
        {
            M11 = cos,
            M21 = sin,
            M12 = -sin,
            M22 = cos,
            M33 = 1,
            M44 = 1
        };

        return t * matrix;
    }

    public static Matrix4x4 Shear(this Matrix4x4 matrix, Vector2 vec)
        => matrix.Shear(vec.X, vec.Y);

    public static Matrix4x4 Shear(this Matrix4x4 matrix, float x, float y)
    {
        var t = new Matrix4x4
        {
            M11 = 1,
            M21 = x,
            M12 = y,
            M22 = 1,
            M33 = 1,
            M44 = 1
        };

        return t * matrix;
    }

    public static Matrix4x4 Scale(this Matrix4x4 matrix, Vector2 vec)
        => matrix.Scale(vec.X, vec.Y);

    public static Matrix4x4 Scale(this Matrix4x4 matrix, float x, float y)
    {
        var t = new Matrix4x4
        {
            M11 = x,
            M22 = y,
            M33 = 1,
            M44 = 1
        };

        return t * matrix;
    }

    public static Matrix4x4 Translate(this Matrix4x4 matrix, Vector2 vec)
        => matrix.Translate(vec.X, vec.Y);

    public static Matrix4x4 Translate(this Matrix4x4 matrix, float x, float y)
    {
        var tmp = Matrix4x4.Identity;
        tmp.M41 = x;
        tmp.M42 = y;

        return tmp * matrix;
    }
}
