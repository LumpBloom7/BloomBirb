using System.Drawing;
using System.Numerics;
using Silk.NET.Maths;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class TextureUsage
{
    public ITexture BackingTexture;

    public readonly bool HasTransparencies;

    public Rectangle<int> TextureRegion;

    public TextureUsage(ITexture backingTexture, Rectangle<int> textureRegion, bool hasTransparencies)
    {
        BackingTexture = backingTexture;
        TextureRegion = textureRegion;
        HasTransparencies = hasTransparencies;
    }

    public Vector2 ToTextureUsageUV(Vector2 uv)
    {
        Vector2 texSize = new(BackingTexture.TextureSize.Width, BackingTexture.TextureSize.Width);
        return (new Vector2(TextureRegion.Origin.X, TextureRegion.Origin.Y) + (uv * new Vector2(TextureRegion.Size.X, TextureRegion.Size.Y))) / texSize;
    }

    public void Bind() => BackingTexture.Bind();

}
