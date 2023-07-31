using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Textures;

public interface ITexture
{
    uint TextureHandle { get; }

    public Vector2D<int> TextureSize { get; }

    bool HasTransparencies { get; }

    void Bind(TextureUnit textureUnit = TextureUnit.Texture0);

    virtual Vector2 ToRegionUV(Vector2 uv) => uv;
}
