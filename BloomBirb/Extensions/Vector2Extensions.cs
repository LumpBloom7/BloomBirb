using System.Numerics;

namespace BloomBirb.Extensions;

public static class Vector2Extensions
{
    public static Vector2 Transform(this Vector2 v, Matrix3 t)
    {
        return new Vector2(
            (v.X * t.M11) + (v.Y * t.M21) + t.M31,
            (v.X * t.M12) + (v.Y * t.M22) + t.M32
        );
    }
}
