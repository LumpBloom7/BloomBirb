using System.Numerics;
using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Batches;
using BloomFramework.Renderers.OpenGL.Textures;

namespace BloomFramework.Graphics;

public class DrawableSprite : Drawable
{
    public override bool IsTranslucent => (Texture?.HasTransparencies ?? false) || base.IsTranslucent;

    public TextureUsage? Texture { get; set; } = null;
    private Shader shader { get; set; }

    public DrawableSprite(Shader shader)
    {
        this.shader = shader;
    }

    public override void QueueDraw(OpenGlRenderer renderer)
    {
        if (Texture is null)
            return;
        base.QueueDraw(renderer);

        renderer.QueueDrawable(this);
    }

    public override void Draw(OpenGlRenderer renderer)
    {
        if (Texture is null)
            return;

        base.Draw(renderer);
        renderer.UseBatch<QuadBatch<DepthWrappingVertex<TexturedVertex2D>>>();

        shader.Bind();
        Texture.Bind();

        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopLeft, DrawColour, new Vector2(0, 1), Texture), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomLeft, DrawColour, new Vector2(0, 0), Texture), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomRight, DrawColour, new Vector2(1, 0), Texture), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopRight, DrawColour, new Vector2(1, 1), Texture), DrawDepth));
    }
}