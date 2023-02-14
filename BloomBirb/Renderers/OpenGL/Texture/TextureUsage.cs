using System.Drawing;
using System.Numerics;
using Silk.NET.Maths;

namespace BloomBirb.Renderers.OpenGL.Textures;

public class TextureUsage
{
    public Texture BackingTexture;

    public readonly bool HasTransparencies;

    public RectangleF TextureRegion;

    public TextureUsage(Texture backingTexture, RectangleF textureRegion, bool hasTransparencies)
    {
        BackingTexture = backingTexture;
        TextureRegion = textureRegion;
        HasTransparencies = hasTransparencies;
    }

    public Vector2 ToTextureUsageUV(Vector2 uv)
        => new Vector2(TextureRegion.Left, TextureRegion.Top) + (uv * ((Vector2)TextureRegion.Size));

    public void Bind() => BackingTexture.Bind();

}
