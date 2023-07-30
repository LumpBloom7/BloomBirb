using System.Numerics;
using BloomFramework.Extensions;
using BloomFramework.Graphics.Primitives;
using BloomFramework.Renderers.OpenGL;

namespace BloomFramework.Graphics;

public abstract class Drawable : IDisposable
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

    private FillMode fillMode = FillMode.None;

    public FillMode FillMode
    {
        get => fillMode;
        set
        {
            if (fillMode == value)
                return;

            Invalidate();
            fillMode = value;
        }
    }

    private float fillRatio = 1;

    public float FillRatio
    {
        get => fillRatio;
        set
        {
            if (Math.Abs(fillRatio - value) < float.Epsilon) return;

            Invalidate();
            fillRatio = value;
        }
    }

    // Draw info
    protected Matrix3 Transformation = Matrix3.Identity;
    protected Quad DrawQuad = Quad.DEFAULT;
    protected Vector4 DrawColour { get; private set; } = Vector4.One;

    internal float DrawDepth;

    public virtual bool IsTranslucent => DrawColour.W < 1f;

    private bool invalidated = true;

    private LoadState loadState = LoadState.NotLoaded;

    internal virtual void LoadInternal()
    {
        if (loadState is LoadState.Loaded or LoadState.Ready)
            return;

        loadState = LoadState.Loading;

        Load();

        loadState = LoadState.Loaded;
    }

    protected virtual void Load()
    {
    }

    internal virtual void UpdateInternal(double dt)
    {
        if (loadState is LoadState.NotLoaded)
        {
            LoadInternal();
        }

        Update(dt);

        loadState = LoadState.Ready;
    }

    protected virtual void Update(double dt)
    {
    }

    public virtual void QueueDraw(OpenGlRenderer renderer)
    {
        if (loadState != LoadState.Ready)
            return;

        if (invalidated)
            Revalidate();
    }

    public virtual void Draw(OpenGlRenderer renderer)
    {
    }

    // Used to keep track of the dimension in screen space pixels
    // So that relative sized children know their screen space size as well
    private Vector2 absoluteSize;

    protected virtual void Revalidate()
    {
        Transformation = Parent?.Transformation ?? Matrix3.Identity;

        Vector2 parentSize = Parent?.absoluteSize ?? Vector2.Zero;

        float anchorOffsetX = 0, anchorOffsetY = 0;

        if (Anchor.HasFlag(Anchor.Top))
            anchorOffsetY = parentSize.Y;
        else if (Anchor.HasFlag(Anchor.Middle))
            anchorOffsetY = parentSize.Y / 2;

        if (Anchor.HasFlag(Anchor.Centre))
            anchorOffsetX = parentSize.X / 2;
        else if (Anchor.HasFlag(Anchor.Right))
            anchorOffsetX = parentSize.X;

        float x = position.X, y = position.Y;

        if (relativePositionAxes.HasFlag(Axes.X))
            x *= parentSize.X;

        if (relativePositionAxes.HasFlag(Axes.Y))
            y *= parentSize.Y;

        x += anchorOffsetX;
        y += anchorOffsetY;

        Matrix3Extensions.Translate(ref Transformation, x, y);
        Matrix3Extensions.RotateDegrees(ref Transformation, Rotation);
        Matrix3Extensions.Shear(ref Transformation, Shear);
        Matrix3Extensions.Scale(ref Transformation, Scale);

        float originOffsetX = 0, originOffsetY = 0;

        float width = size.X, height = size.Y;

        if (RelativeSizeAxes.HasFlag(Axes.X))
            width *= parentSize.X;
        if (RelativeSizeAxes.HasFlag(Axes.Y))
            height *= parentSize.Y;

        var fillModeScale = computeFillModeSize(width, height);

        width = fillModeScale.X;
        height = fillModeScale.Y;

        absoluteSize = new Vector2(width, height);

        if (Origin.HasFlag(Anchor.Top))
            originOffsetY = -height;
        else if (Origin.HasFlag(Anchor.Middle))
            originOffsetY = -height / 2;

        if (Origin.HasFlag(Anchor.Centre))
            originOffsetX = -width / 2;
        else if (Origin.HasFlag(Anchor.Right))
            originOffsetX = -width;

        Matrix3Extensions.Translate(ref Transformation, originOffsetX, originOffsetY);

        DrawColour = (Parent?.DrawColour ?? Vector4.One) * Colour * new Vector4(1, 1, 1, Alpha);

        DrawQuad = new Quad(0, 0, width, height) * Transformation;
        invalidated = false;
    }

    // Computes the size of the drawable AFTER applying appropriate fill modes
    private Vector2 computeFillModeSize(float width, float height)
    {
        if (fillMode is FillMode.None)
            return new Vector2(width, height);

        Vector2 result = Vector2.One;
        if (fillMode is FillMode.Fill)
            result = new Vector2(Math.Max(width, height * fillRatio));
        else if (FillMode is FillMode.Fit)
            result = new Vector2(Math.Min(width, height * fillRatio));

        return result with { Y = result.Y / fillRatio };
    }

    public virtual void Invalidate()
    {
        invalidated = true;
    }

    public bool Disposed { get; private set; }

    protected virtual void Dispose(bool isDisposing)
    {
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        Dispose(true);

        Disposed = true;

        GC.SuppressFinalize(this);
    }

    ~Drawable()
    {
        Dispose(false);
    }
}
