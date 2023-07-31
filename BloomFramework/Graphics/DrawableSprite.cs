using System.Diagnostics;
using System.Numerics;
using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Buffers;
using BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;
using BloomFramework.Renderers.OpenGL.Textures;

namespace BloomFramework.Graphics;

public class DrawableSprite : Drawable
{
    public override bool IsTranslucent => (Texture?.HasTransparencies ?? false) || base.IsTranslucent;

    public ITexture? Texture { get; set; } = null;
    private Shader shader { get; set; }

    public DrawableSprite(Shader shader)
    {
        this.shader = shader;
    }

    public override void QueueDraw(OpenGlRenderer renderer)
    {
        Texture ??= renderer.BlankTexture;

        base.QueueDraw(renderer);

        renderer.QueueDrawable(this);
    }

    public override void Draw(OpenGlRenderer renderer)
    {
        Debug.Assert(Texture is not null);

        base.Draw(renderer);
        renderer.UseBuffer<VertexBuffer<DepthWrappingVertex<TexturedVertex2D>, QuadElementBuffer>>();

        shader.Bind();
        Texture.Bind();

        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopLeft, DrawColour, Texture.ToRegionUV(new Vector2(0, 1))), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomLeft, DrawColour, Texture.ToRegionUV(new Vector2(0, 0))), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.BottomRight, DrawColour, Texture.ToRegionUV(new Vector2(1, 0))), DrawDepth));
        renderer.AddVertex(new DepthWrappingVertex<TexturedVertex2D>(new(DrawQuad.TopRight, DrawColour, Texture.ToRegionUV(new Vector2(1, 1))), DrawDepth));
    }
}
