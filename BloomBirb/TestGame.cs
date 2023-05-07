using System.Numerics;
using BloomBirb.Fonts.BMFont;
using BloomBirb.Graphics;
using BloomBirb.Graphics.Text;
using BloomBirb.ResourceStores;

namespace BloomBirb;

public class TestGame : GameBase
{
    private EmbeddedResourceStore resources = null!;
    private ShaderStore shaders = null!;
    private TextureStore textures = null!;
    private AudioStore audioStore = null!;
    private Font font = null!;

    private DrawableSprite box = null!;

    private SpriteText frameTimeText = null!;

    // Load everything needed for the scene to function
    // Also loads children afterwards without additional calls
    protected override void Load()
    {
        base.Load();

        // Load resources based on what we need
        // In this case audio store is optional
        resources = new EmbeddedResourceStore();
        shaders = new ShaderStore(renderer, resources);
        textures = new TextureStore(renderer, resources);
        audioStore = new AudioStore(resources);

        // Load a font from our resources
        var fontTextures = new TextureStore(renderer, resources, "Fonts");
        font = new Font(resources.Get("Fonts.OpenSans.fnt")!, fontTextures);

        // Declare required shader for sprites (This may be automated soon with this as the default)
        var spriteShader = shaders.Get("Texture", "Texture");
        spriteShader.Bind();
        spriteShader.SetUniform("u_Texture0", 0);

        // Add our drawables
        AddRange(new Drawable[]
        {
            box = new DrawableSprite(spriteShader)
            {
                Texture = textures.Get("something"),
                Size = new(300f),
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre
            },
            frameTimeText = new SpriteText(spriteShader, font)
            {
                Scale = new Vector2(0.33f),
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft
            }
        });
    }

    private float hue = 0;
    private double timeTrack = 0;

    // Update drawables' properties, this may be run on a separate thread, allowing for lower latencies.
    // Delta time is measured in seconds
    protected override void Update(double dt)
    {
        base.Update(dt);

        frameTimeText.Text = $"Frametime: {dt * 1000:F2}ms";
        box.Rotation += (float)(dt * 90);
        hue += (float)(dt * 0.125f);

        box.Colour = HsvToRgb(hue % 1, 1, 1);
        timeTrack += dt;

        if (!(timeTrack > 2)) return;
        timeTrack %= 2;

        // Add a random box to the scene every 2 secs
        // Note that these sprites aren't "loaded", since they are just created
        // These will be loaded on the first use, by the framework
        Add(new DrawableSprite(shaders.Get("Texture", "Texture"))
        {
            Texture = textures.Get("a load of this"),
            Size = new Vector2(Random.Shared.NextSingle() * 300),
            RelativePositionAxes = Axes.Both,
            Anchor = Anchor.MiddleCentre,
            Origin = Anchor.MiddleCentre,
            Position = new Vector2(Random.Shared.NextSingle() - 0.5f, Random.Shared.NextSingle() - 0.5f),
            Rotation = Random.Shared.NextSingle() * 360,
            Colour = HsvToRgb(Random.Shared.NextSingle(), 1, 1)
        });
    }

    private static Vector4 HsvToRgb(float h, float s, float v)
    {
        h *= 360;

        float f(float n)
        {
            float k = (n + h / 60) % 6;
            return v - v * s * MathF.Max(0, MathF.Min(k, MathF.Min(4 - k, 1)));
        }

        return new Vector4(f(5), f(3), f(1), 1);
    }
}
