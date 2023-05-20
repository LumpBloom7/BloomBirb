using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class TextureUsage : ITextureUsage
{
    public ITexture BackingTexture { get; }
    public bool HasTransparencies { get; private init; }
    public Vector2 RegionOrigin { get; private init; }
    public virtual Vector2 RegionSize { get; private init; }

    public TextureUsage(ITexture backingTexture, Rectangle<int> textureRegion, bool hasTransparencies)
    {
        BackingTexture = backingTexture;

        var normalisationFactor = new Vector2(1f/backingTexture.TextureSize.X, 1f/backingTexture.TextureSize.Y);

        RegionOrigin = new Vector2(textureRegion.Origin.X, textureRegion.Origin.Y) * normalisationFactor;
        RegionSize = new Vector2(textureRegion.Size.X, textureRegion.Size.Y) * normalisationFactor;

        HasTransparencies = hasTransparencies;
    }

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0) => BackingTexture.Bind(textureUnit);
}
