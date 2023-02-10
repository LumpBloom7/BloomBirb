using System.Numerics;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Batches;

namespace BloomBirb.Graphics;

public class DrawableSprite : Drawable
{
    private Texture texture { get; set; }
    private Shader shader { get; set; }

    public DrawableSprite(Texture texture, Shader shader)
    {
        this.texture = texture;
        this.shader = shader;
    }

    public override void Draw(OpenGLRenderer renderer, QuadBatch<DepthWrappingVertex<TexturedVertex2D>> quadBuffer)
    {
        base.Draw(renderer, quadBuffer);
        shader.Bind();
        texture.Bind();

        bool isOpaque = !texture.HasTransparencies && DrawColour.W == 1f;

        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.TopLeft, DrawColour, new Vector2(0, 1)), isOpaque);
        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.BottomLeft, DrawColour, new Vector2(0, 0)), isOpaque);
        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.BottomRight, DrawColour, new Vector2(1, 0)), isOpaque);
        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.TopRight, DrawColour, new Vector2(1, 1)), isOpaque);
    }
}
