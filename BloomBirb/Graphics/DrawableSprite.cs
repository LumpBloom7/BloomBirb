using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Batches;

namespace BloomBirb.Graphics;

public class DrawableSprite : Drawable
{
    public override bool IsTranslucent => texture.HasTransparencies || base.IsTranslucent;

    private Texture texture { get; set; }
    private Shader shader { get; set; }

    public DrawableSprite(Texture texture, Shader shader)
    {
        this.texture = texture;
        this.shader = shader;
    }

    public override void Draw(OpenGLRenderer renderer)
    {
        base.Draw(renderer);
        renderer.UseBatch<QuadBatch<DepthWrappingVertex<TexturedVertex2D>>>();
        shader.Bind();
        texture.Bind();

        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopLeft, DrawColour, new(0, 1)), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomLeft, DrawColour, new(0, 0)), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomRight, DrawColour, new(1, 0)), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopRight, DrawColour, new(1, 1)), DrawDepth));
    }
}
