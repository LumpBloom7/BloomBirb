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

    public override void Draw(GL context)
    {
        base.Draw(context);
        shader.Use();
        texture.Bind();

        shader.SetUniform("u_TransMat", Transformation);
        shader.SetUniform("u_Color", DrawColour);


        shader?.SetUniform("u_Texture0", 0);
        shader?.SetUniform("u_Circle", 0);
        shader?.SetUniform("u_ScreenSpaceCentreX", 400f);
        shader?.SetUniform("u_ScreenSpaceCentreY", 300f);
        shader?.SetUniform("u_CircleRadius", 150f);
    }
}
