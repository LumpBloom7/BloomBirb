using System.Numerics;

namespace BloomBirb.Extensions;

public static class Matrix4Extensions
{
    public static Matrix3 RotateDegrees(this Matrix3 matrix, float degrees)
        => matrix.RotateRadians(degrees * (float.Pi / 180));

    public static Matrix3 RotateRadians(this Matrix3 matrix, float radians)
    {
        (float sin, float cos) = MathF.SinCos(radians);

        var t = new Matrix3
        {
            M11 = cos,
            M21 = sin,
            M12 = -sin,
            M22 = cos,
            M33 = 1,
        };

        return t * matrix;
    }

    public static Matrix3 Shear(this Matrix3 matrix, Vector2 vec)
        => matrix.Shear(vec.X, vec.Y);

    public static Matrix3 Shear(this Matrix3 matrix, float x, float y)
    {
        var t = new Matrix3
        {
            M11 = 1,
            M21 = x,
            M12 = y,
            M22 = 1,
            M33 = 1,
        };

        return t * matrix;
    }

    public static Matrix3 Scale(this Matrix3 matrix, Vector2 vec)
        => matrix.Scale(vec.X, vec.Y);

    public static Matrix3 Scale(this Matrix3 matrix, float x, float y)
    {
        var t = new Matrix3
        {
            M11 = x,
            M22 = y,
            M33 = 1,
        };

        return t * matrix;
    }

    public static Matrix3 Translate(this Matrix3 matrix, Vector2 vec)
        => matrix.Translate(vec.X, vec.Y);

    public static Matrix3 Translate(this Matrix3 matrix, float x, float y)
    {
        var tmp = Matrix3.Identity;
        tmp.M31 = x;
        tmp.M32 = y;

        return tmp * matrix;
    }
}
