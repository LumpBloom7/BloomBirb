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

    private Anchor anchor = Anchor.BottomLeft;

    public Anchor Anchor
    {
        get => anchor;
        set
        {
            if (anchor == value) return;

            Invalidate();
            anchor = value;
        }
    }

    private Anchor origin = Anchor.BottomLeft;

    public Anchor Origin
    {
        get => origin;
        set
        {
            if (origin == value) return;

            Invalidate();
            origin = value;
        }
    }

    private Axes relativeSizeAxes = Axes.None;

    public Axes RelativeSizeAxes
    {
        get => relativeSizeAxes;
        set
        {
            if (relativeSizeAxes == value) return;

            Invalidate();
            relativeSizeAxes = value;
        }
    }

    private Axes relativePositionAxes = Axes.None;

    public Axes RelativePositionAxes
    {
        get => relativePositionAxes;
        set
        {
            if (relativePositionAxes == value) return;

            Invalidate();
            relativePositionAxes = value;
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
        if (invalidated)
            Revalidate();
    }

    public virtual void Draw(OpenGlRenderer renderer)
    {
    }

    protected virtual void Revalidate()
    {
        Transformation = Parent?.Transformation ?? Matrix3.Identity;

        Vector2 parentSize = Parent?.size ?? Vector2.Zero;

        float anchorOffsetX = 0, anchorOffsetY = 0;

        if (Anchor.HasFlag(Anchor.Top))
            anchorOffsetY = parentSize.Y;
        else if (Anchor.HasFlag(Anchor.Middle))
            anchorOffsetY = parentSize.Y / 2;

        if (Anchor.HasFlag(Anchor.Centre))
            anchorOffsetX = parentSize.X / 2;
        else if (Anchor.HasFlag(Anchor.Right))
            anchorOffsetX = parentSize.X;

        float posX = position.X, posY = position.Y;

        if (relativePositionAxes.HasFlag(Axes.X))
            posX *= parentSize.X;

        if (relativePositionAxes.HasFlag(Axes.Y))
            posY *= parentSize.Y;

        posX += anchorOffsetX;
        posY += anchorOffsetY;

        Matrix3Extensions.Translate(ref Transformation, posX, posY);
        Matrix3Extensions.RotateDegrees(ref Transformation, Rotation);
        Matrix3Extensions.Shear(ref Transformation, Shear);
        Matrix3Extensions.Scale(ref Transformation, Scale);

        float originOffsetX = 0, originOffsetY = 0;

        float sizeX = size.X, sizeY = size.Y;

        if (RelativeSizeAxes.HasFlag(Axes.X))
            sizeX *= parentSize.X;
        if (RelativeSizeAxes.HasFlag(Axes.Y))
            sizeY *= parentSize.Y;

        if (Origin.HasFlag(Anchor.Top))
            originOffsetY = -sizeY;
        else if (Origin.HasFlag(Anchor.Middle))
            originOffsetY = -sizeY / 2;

        if (Origin.HasFlag(Anchor.Centre))
            originOffsetX = -sizeX / 2;
        else if (Origin.HasFlag(Anchor.Right))
            originOffsetX = -sizeX;

        Matrix3Extensions.Translate(ref Transformation, originOffsetX, originOffsetY);

        DrawColour = (Parent?.DrawColour ?? Vector4.One) * Colour * new Vector4(1, 1, 1, Alpha);

        DrawQuad = new Quad(0, 0, sizeX, sizeY) * Transformation;
        invalidated = false;
    }

    public virtual void Invalidate()
    {
        invalidated = true;
    }
}
