using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class TextureRegion : ITexture
{
    private readonly ITexture backingTexture;

    private readonly Vector2 offset;
    private readonly Vector2 uvRange;

    private readonly Rectangle<int> region;

    public bool HasTransparencies { get; private set; }

    public TextureRegion(ITexture backingTexture, Rectangle<int> region, bool hasTransparencies = false)
    {
        this.backingTexture = backingTexture;
        this.region = region;
        HasTransparencies = hasTransparencies;

        var backingOffset = backingTexture.ToRegionUV(Vector2.Zero);
        var backingUVSize = backingTexture.ToRegionUV(Vector2.One) - backingOffset;

        var normalisationFactor = backingUVSize / new Vector2(backingTexture.TextureSize.X, backingTexture.TextureSize.Y);

        uvRange = new Vector2(region.Size.X, region.Size.Y) * normalisationFactor;
        offset = backingOffset + new Vector2(region.Origin.X, region.Origin.Y) * normalisationFactor;
    }

    public uint TextureHandle => backingTexture.TextureHandle;

    public Vector2D<int> TextureSize => region.Size;

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0) => backingTexture.Bind(textureUnit);

    public Vector2 ToRegionUV(Vector2 uv) => (uv * uvRange) + offset;
}
