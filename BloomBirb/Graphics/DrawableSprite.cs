using System.Numerics;
using BloomBirb.Graphics.Primitives;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Buffers;
using Silk.NET.OpenGL;

using Shader = BloomBirb.Renderers.OpenGL.Shader;
using Texture = BloomBirb.Renderers.OpenGL.Texture;

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

    public override void Draw(OpenGLRenderer renderer, QuadBuffer<TexturedVertex2D> quadBuffer)
    {
        base.Draw(renderer, quadBuffer);
        shader.Bind();
        texture.Bind();

        shader.SetUniform("u_Color", DrawColour);

        shader?.SetUniform("u_Texture0", 0);
        shader?.SetUniform("u_Circle", 0);
        shader?.SetUniform("u_ScreenSpaceCentreX", 400f);
        shader?.SetUniform("u_ScreenSpaceCentreY", 300f);
        shader?.SetUniform("u_CircleRadius", 150f);

        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.TopLeft, new Vector2(0, 1)));
        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.BottomLeft, new Vector2(0, 0)));
        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.BottomRight, new Vector2(1, 0)));
        quadBuffer.AddVertex(new TexturedVertex2D(DrawQuad.TopRight, new Vector2(1, 1)));
    }
}
