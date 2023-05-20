using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Textures;

public interface ITexture
{
    uint TextureHandle { get; }

    public Vector2D<int> TextureSize { get; }

    void Bind(TextureUnit textureUnit = TextureUnit.Texture0);
}
