using BloomFramework.Fonts.BMFont;
using BloomFramework.Graphics.Text;
using BloomFramework.Renderers.OpenGL;

namespace BlackLotus.Components;

public class FrametimeCounter : SpriteText
{
    public FrametimeCounter(Shader shader, Font font) : base(shader, font)
    {
    }

    protected override void Update(double dt)
    {
        base.Update(dt);

        Text = $"Frametime: {(dt * 1000):F2}ms";
    }
}
