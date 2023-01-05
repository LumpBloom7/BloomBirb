using System;
using Silk.NET.Maths;

namespace BloomBirb.Extensions;

public static class Matrix4Extensions
{
    public static Matrix4X4<float> RotateDegrees(this Matrix4X4<float> matrix, float degrees)
        => matrix.RotateRadians(degrees * (float.Pi / 180));

    public static Matrix4X4<float> RotateRadians(this Matrix4X4<float> matrix, float radians)
    {

        (float sin, float cos) = MathF.SinCos(radians);
        var result = new Matrix4X4<float>
        {
            M11 = (cos * matrix.M11) + (-sin * matrix.M21),
            M12 = (cos * matrix.M12) + (-sin * matrix.M22),
            M21 = (sin * matrix.M11) + (cos * matrix.M21),
            M22 = (sin * matrix.M12) + (cos * matrix.M22),
            M33 = 1,
            M14 = matrix.M14,
            M24 = matrix.M24,
            M34 = matrix.M34,
            M44 = 1,
        };

        return result;
    }
    public static Matrix4X4<float> Shear(this Matrix4X4<float> matrix, float x, float y)
    {
        var result = new Matrix4X4<float>
        {
            M11 = ((1 + x * y) * matrix.M11) + (y * matrix.M21),
            M12 = ((1 + x * y) * matrix.M12) + (y * matrix.M22),
            M21 = (x * matrix.M11) + matrix.M21,
            M22 = (x * matrix.M12) + matrix.M22,
            M33 = 1,
            M14 = matrix.M14,
            M24 = matrix.M24,
            M34 = matrix.M34,
            M44 = 1,
        };

        return result;
    }

    public static Matrix4X4<float> Scale(this Matrix4X4<float> matrix, float x, float y)
    {
        var result = new Matrix4X4<float>
        {
            M11 = x * matrix.M11,
            M12 = x * matrix.M12,
            M21 = y * matrix.M21,
            M22 = y * matrix.M22,
            M33 = 1,
            M14 = matrix.M14,
            M24 = matrix.M24,
            M34 = matrix.M34,
            M44 = 1,
        };

        return result;
    }

    public static Matrix4X4<float> Translate(this Matrix4X4<float> matrix, float x, float y)
    {
        var tmp = Matrix4X4<float>.Identity;
        tmp.M13 = x;
        tmp.M13 = y;

        return tmp * matrix;
    }
}
