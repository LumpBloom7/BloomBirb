using System.Numerics;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Textures;

public interface ITextureUsage
{
    ITexture BackingTexture { get; }
    bool HasTransparencies { get; }
    Vector2 RegionOrigin { get; }

    Vector2 RegionSize { get; }

    void Bind(TextureUnit textureUnit = TextureUnit.Texture0);
}
