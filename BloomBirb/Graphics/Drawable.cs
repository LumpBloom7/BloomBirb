using System.Numerics;
using BloomBirb.Extensions;
using BloomBirb.Graphics.Primitives;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Batches;

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
    protected Matrix3 Transformation = Matrix3.Identity;
    protected Quad DrawQuad = Quad.DEFAULT;
    protected Vector4 DrawColour { get; private set; } = Vector4.One;

    public virtual void Draw(OpenGLRenderer renderer, QuadBatch<DepthWrappingVertex<TexturedVertex2D>> quadBuffer)
    {
    }

    public void Invalidate()
    {
        Transformation = Parent?.Transformation ?? Matrix3.Identity;
        Matrix3Extensions.RotateDegrees(ref Transformation, Rotation);
        Matrix3Extensions.Shear(ref Transformation, Shear);
        Matrix3Extensions.Translate(ref Transformation, Position);
        Matrix3Extensions.Scale(ref Transformation, Scale);

        DrawColour = (Parent?.DrawColour ?? Vector4.One) * Colour * new Vector4(1, 1, 1, Alpha);
        DrawQuad = Quad.DEFAULT * Transformation;
    }
}
