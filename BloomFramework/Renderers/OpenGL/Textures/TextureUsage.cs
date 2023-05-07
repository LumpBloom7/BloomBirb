using Silk.NET.Maths;

namespace BloomFramework.Renderers.OpenGL.Textures;

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

    public void Bind() => BackingTexture.Bind();

}
