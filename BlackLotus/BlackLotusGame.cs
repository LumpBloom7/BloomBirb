using System.Numerics;
using BlackLotus.Components;
using BloomFramework;
using BloomFramework.Fonts.BMFont;
using BloomFramework.Graphics;
using BloomFramework.Graphics.Containers;
using BloomFramework.ResourceStores;

namespace BlackLotus;

public class BlackLotusGame : GameBase
{
    private IResourceStore resourceStore =
        new FallbackResourceStore(new EmbeddedResourceStore(typeof(BlackLotusGame).Assembly,"BlackLotus.Resources"));

    private ShaderStore shaders = null!;
    private TextureStore textures = null!;


    private Container contentLayer;
    private Container overlayLayer;

    protected override void Load()
    {
        base.Load();

        shaders = new ShaderStore(renderer, resourceStore);
        textures = new TextureStore(renderer, resourceStore);

        var fontTextures = new TextureStore(renderer, resourceStore, "Fonts");
        var font = new Font(resourceStore.Get("Fonts.Monogram.fnt")!, fontTextures);

        var textureShader = shaders.Get("Texture", "Texture");
        textureShader.SetUniform("u_Texture0", 0);

        AddRange(new Drawable[]
        {
            contentLayer = new Container
            {
                RelativeSizeAxes = Axes.Both
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

        contentLayer.Add(new DrawableSprite(textureShader)
        {
            Texture = textures.Get("Backgrounds.BG1"),
            RelativeSizeAxes = Axes.Both
        });
        contentLayer.Add(new DrawableSprite(textureShader)
        {
            Texture = textures.Get("Backgrounds.BG2"),
            RelativeSizeAxes = Axes.Both
        });
        contentLayer.Add(new DrawableSprite(textureShader)
        {
            Texture = textures.Get("Backgrounds.BG3"),
            RelativeSizeAxes = Axes.Both
        });
    }
}
