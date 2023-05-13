using System.Numerics;
using BlackLotus.Components;
using BloomFramework;
using BloomFramework.Fonts.BMFont;
using BloomFramework.Graphics;
using BloomFramework.Graphics.Containers;
using BloomFramework.Graphics.Textures;
using BloomFramework.ResourceStores;

namespace BlackLotus;

public class BlackLotusGame : GameBase
{
    private IResourceStore resourceStore =
        new FallbackResourceStore(new EmbeddedResourceStore(typeof(BlackLotusGame).Assembly, "BlackLotus.Resources"));

    private ShaderStore shaders = null!;
    private TextureStore textures = null!;

    private Container contentLayer = null!;
    private Container overlayLayer = null!;

    private Container sunLayer = null!;
    private Drawable kitty = null!;

    protected override void Load()
    {
        base.Load();

        shaders = new ShaderStore(renderer, resourceStore);
        textures = new TextureStore(renderer, resourceStore, filterMode: FilterMode.Nearest);

        var fontTextures = new TextureStore(renderer, resourceStore, "Fonts");
        var font = new Font(resourceStore.Get("Fonts.Monogram.fnt")!, fontTextures);

        var textureShader = shaders.Get("Texture", "Texture");

        AddRange(new Drawable[]
        {
            contentLayer = new Container
            {
                RelativeSizeAxes = Axes.Both,
            },
            overlayLayer = new Container
            {
                RelativeSizeAxes = Axes.Both
            }
        });

        overlayLayer.Add(new FrametimeCounter(textureShader, font)
        {
            Scale = new Vector2(0.33f)
        });

        Container kittyLayer;
        contentLayer.AddRange(new Drawable[]
        {
            new DrawableSprite(textureShader)
            {
                Texture = textures.Get("Backgrounds.BG1"),
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillRatio = 320f/192f,
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre
            },
            sunLayer = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f),
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre
            },
            new DrawableSprite(textureShader)
            {
                Texture = textures.Get("Backgrounds.BG2"),
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillRatio = 320f/192f,
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre
            },
            kittyLayer = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre
            },
            new DrawableSprite(textureShader)
            {
                Texture = textures.Get("Backgrounds.BG3"),
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillRatio = 320f/192f,
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre
            }
        });

        sunLayer.Add(new DrawableSprite(textureShader)
        {
            Size = new Vector2(100),
            Colour = new Vector4(1, 0.4f, 0.4f, 1),
            Anchor = Anchor.TopCentre,
            Origin = Anchor.MiddleCentre,
        });

        kittyLayer.Add(kitty = new DrawableSprite(textureShader)
        {
            Size = new Vector2(200),
            Anchor = Anchor.MiddleRight,
            Origin = Anchor.MiddleRight,
            Texture = textures.Get("kitty"),
            RelativePositionAxes = Axes.Y
        });
    }

    private double kitrTime = 0;

    protected override void Update(double dt)
    {
        base.Update(dt);

        kitrTime += dt * 0.5;

        sunLayer.Rotation += (float)(dt * 50);
        kitty.Position = new Vector2(0, (float)Math.Sin(kitrTime)/10);
    }
}
