using BloomFramework.Fonts.BMFont;
using BloomFramework.Graphics.Text;
using BloomFramework.Renderers.OpenGL;

namespace BlackLotus.Components;

public class FrametimeCounter : SpriteText
{
    public FrametimeCounter(Shader shader, Font font) : base(shader, font)
    {
    }

    private int frames = 0;
    private double accumulator = 0;

    protected override void Update(double dt)
    {
        base.Update(dt);

        ++frames;
        accumulator += dt;

        if (!(accumulator >= 0.5)) return;

        Text = $"Frametime: {((accumulator / frames) * 1000):F2} | {Math.Floor(frames / accumulator):F0} FPS";
        accumulator = 0;
        frames = 0;
    }
}
