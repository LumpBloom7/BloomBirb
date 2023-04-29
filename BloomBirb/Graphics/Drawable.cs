using System.Numerics;
using BloomBirb.Extensions;
using BloomBirb.Graphics.Primitives;
using BloomBirb.Renderers.OpenGL;

namespace BloomBirb.Graphics;

public abstract class Drawable
{
    // Used for inheritance
    private Drawable? parent;
    public Drawable? Parent
    {
        get => parent;
        internal set
        {
            if (parent == value) return;

            Invalidate();
            parent = value;
        }
    }

    private Vector2 position = Vector2.Zero;
    public Vector2 Position
    {
        get => position;
        set
        {
            if (position == value) return;

            Invalidate();
            position = value;
        }
    }

    private float alpha = 1;
    public float Alpha
    {
        get => alpha;
        set
        {
            if (Math.Abs(alpha - value) < 0.01) return;

            Invalidate();
            alpha = value;
        }
    }

    private Vector4 colour = Vector4.One;
    public Vector4 Colour
    {
        get => colour;
        set
        {
            if (colour == value) return;

            Invalidate();
            colour = value;
        }
    }

    private Vector2 size = Vector2.One;
    public Vector2 Size
    {
        get => size;
        set
        {
            if (size == value) return;

            Invalidate();
            size = value;
        }
    }

    private Vector2 scale = Vector2.One;
    public Vector2 Scale
    {
        get => scale;
        set
        {
            if (scale == value) return;

            Invalidate();
            scale = value;
        }
    }

    private Vector2 shear = Vector2.Zero;
    public Vector2 Shear
    {
        get => shear;
        set
        {
            if (shear == value) return;

            Invalidate();
            shear = value;
        }
    }

    private float rotation;
    public float Rotation
    {
        get => rotation;
        set
        {
            if (Math.Abs(rotation - value) < 0.01) return;

            Invalidate();
            rotation = value;
        }
    }

    // Draw info
    protected Matrix3 Transformation = Matrix3.Identity;
    protected Quad DrawQuad = Quad.DEFAULT;
    protected Vector4 DrawColour { get; private set; } = Vector4.One;

    internal float DrawDepth;

    public virtual bool IsTranslucent => DrawColour.W < 1f && DrawColour.W > 0.1f;

    private bool invalidated = true;

    public virtual void QueueDraw(OpenGlRenderer renderer)
    {
    }

    public virtual void Draw(OpenGlRenderer renderer)
    {
        if (invalidated)
            revalidate();
    }

    private void revalidate()
    {
        Transformation = Parent?.Transformation ?? Matrix3.Identity;
        Matrix3Extensions.Translate(ref Transformation, Position);
        Matrix3Extensions.RotateDegrees(ref Transformation, Rotation);
        Matrix3Extensions.Shear(ref Transformation, Shear);
        Matrix3Extensions.Scale(ref Transformation, Scale);

        DrawColour = (Parent?.DrawColour ?? Vector4.One) * Colour * new Vector4(1, 1, 1, Alpha);

        DrawQuad = new Quad(0, 0, Size.X, Size.Y) * Transformation;
        invalidated = false;
    }


    public virtual void Invalidate()
    {
        invalidated = true;
    }
}
