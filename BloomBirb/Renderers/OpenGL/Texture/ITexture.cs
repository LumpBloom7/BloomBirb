using SixLabors.ImageSharp;

namespace BloomBirb.Renderers.OpenGL;

public interface ITexture
{
    uint TextureHandle { get; }
    public Size TextureSize { get; }

    void Bind();
}
