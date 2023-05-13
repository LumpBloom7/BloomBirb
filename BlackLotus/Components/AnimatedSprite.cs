using BloomFramework.Graphics;
using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Textures;

namespace BlackLotus.Components;

public class AnimatedSprite : DrawableSprite
{
    private readonly TextureUsage[] sprites;
    private readonly float frameDelta;

    private int index;

    public AnimatedSprite(Shader shader, TextureUsage[] frames, float fps) : base(shader)
    {
        sprites = frames;
        frameDelta = 1 / fps;
    }

    private double timeAccumulator;

    protected override void Update(double dt)
    {
        base.Update(dt);

        timeAccumulator += dt;

        while (timeAccumulator >= frameDelta)
        {
            index++;
            timeAccumulator -= frameDelta;
        }

        index %= sprites.Length;
        Texture = sprites[index];
    }
}
