using System.Numerics;
using BloomBirb.Extensions;
using Silk.NET.OpenGL;

namespace BloomBirb.Graphics;

public abstract class Drawable
{
    // Used for inheritance
    protected Drawable? Parent { get; set; } = null;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public float Alpha { get; set; } = 1;

    public Vector4 Colour { get; set; } = Vector4.One;

    public Vector2 Size { get; set; } = Vector2.One;

    public Vector2 Scale { get; set; } = Vector2.One;

    public Vector2 Shear { get; set; } = Vector2.Zero;

    public float Rotation { get; set; } = 0;

    // Draw info
    protected Matrix4x4 Transformation { get; private set; } = Matrix4x4.Identity;
    protected Vector4 DrawColour { get; private set; } = Vector4.One;

    public virtual void Draw(GL context)
    {
        context.Enable(GLEnum.Blend);
        context.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
    }

    public void Invalidate()
    {
        Transformation = (Parent?.Transformation ?? Matrix4x4.Identity).RotateDegrees(Rotation).Shear(Shear).Scale(Size * Scale).Translate(Position);
        DrawColour = (Parent?.DrawColour ?? Vector4.One) * Colour * new Vector4(1, 1, 1, Alpha);
    }
}
