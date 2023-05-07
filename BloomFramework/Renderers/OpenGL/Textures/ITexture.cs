using SixLabors.ImageSharp;

namespace BloomFramework.Renderers.OpenGL.Textures;

public interface ITexture
{
    uint TextureHandle { get; }
    public Size TextureSize { get; }

    void Bind();
}
