using System.Numerics;
using Silk.NET.Maths;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class TextureUsage
{
    public readonly ITexture BackingTexture;

    public readonly bool HasTransparencies;

    public Vector2 RegionOrigin { get; private init; }
    public Vector2 RegionSize { get; private init; }

    public TextureUsage(ITexture backingTexture, Rectangle<int> textureRegion, bool hasTransparencies)
    {
        BackingTexture = backingTexture;

        var normalisationFactor = new Vector2(1f/backingTexture.TextureSize.Width, 1f/backingTexture.TextureSize.Height);

        RegionOrigin = new Vector2(textureRegion.Origin.X, textureRegion.Origin.Y) * normalisationFactor;
        RegionSize = new Vector2(textureRegion.Size.X, textureRegion.Size.Y) * normalisationFactor;

        HasTransparencies = hasTransparencies;
    }

    public void Bind() => BackingTexture.Bind();
}
