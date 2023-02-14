using System.Numerics;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Batches;
using BloomBirb.Renderers.OpenGL.Textures;

namespace BloomBirb.Graphics;

public class DrawableSprite : Drawable
{
    public override bool IsTranslucent => texture.HasTransparencies || base.IsTranslucent;

    private TextureUsage texture { get; set; }
    private Shader shader { get; set; }

    public DrawableSprite(TextureUsage texture, Shader shader)
    {
        this.texture = texture;
        this.shader = shader;
    }

    public override void QueueDraw(OpenGLRenderer renderer)
    {
        renderer.QueueDrawable(this, shader, texture, IsTranslucent);
    }

    public override void Draw(OpenGLRenderer renderer)
    {
        base.Draw(renderer);
        renderer.UseBatch<QuadBatch<DepthWrappingVertex<TexturedVertex2D>>>();
        shader.Bind();
        texture.Bind();

        var topLeft = texture.ToTextureUsageUV(new Vector2(0, 1));
        var bottomLeft = texture.ToTextureUsageUV(new Vector2(0, 0));
        var bottomRight = texture.ToTextureUsageUV(new Vector2(1, 0));
        var topRight = texture.ToTextureUsageUV(new Vector2(1, 1));

        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopLeft, DrawColour, topLeft), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomLeft, DrawColour, bottomLeft), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomRight, DrawColour, bottomRight), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopRight, DrawColour, topRight), DrawDepth));
    }
}
