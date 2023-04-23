using System.Numerics;
using BloomBirb.Extensions;
using BloomBirb.Graphics.Primitives;
using BloomBirb.Renderers.OpenGL;

namespace BloomBirb.Graphics;

public abstract class Drawable
{
    // Used for inheritance
    public Drawable? Parent { get; internal set; } = null;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public float Alpha { get; set; } = 1;

    public Vector4 Colour { get; set; } = Vector4.One;

    public Vector2 Size { get; set; } = Vector2.One;

    public Vector2 Scale { get; set; } = Vector2.One;

    public Vector2 Shear { get; set; } = Vector2.Zero;

    public float Rotation { get; set; } = 0;

    // Draw info
    protected Matrix3 Transformation = Matrix3.Identity;
    protected Quad DrawQuad = Quad.DEFAULT;
    protected Vector4 DrawColour { get; private set; } = Vector4.One;

    internal float DrawDepth;

    public virtual bool IsTranslucent => DrawColour.W < 1f && DrawColour.W > 0.1f;

    public virtual void QueueDraw(OpenGlRenderer renderer)
    {
    }

    public virtual void Draw(OpenGlRenderer renderer)
    {
    }

    public virtual void Invalidate()
    {
        Transformation = Parent?.Transformation ?? Matrix3.Identity;
        Matrix3Extensions.Translate(ref Transformation, Position);
        Matrix3Extensions.RotateDegrees(ref Transformation, Rotation);
        Matrix3Extensions.Shear(ref Transformation, Shear);
        Matrix3Extensions.Scale(ref Transformation, Scale);

        DrawColour = (Parent?.DrawColour ?? Vector4.One) * Colour * new Vector4(1, 1, 1, Alpha);

        DrawQuad = new Quad(0, 0, Size.X, Size.Y) * Transformation;
    }
}
