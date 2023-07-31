using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomFramework.Renderers.OpenGL.Textures;

public sealed class TextureWhitePixel : ITexture, IDisposable
{
    private Texture fallbackPixel;
    private OpenGlRenderer renderer;

    public TextureWhitePixel(OpenGlRenderer renderer)
    {
        this.renderer = renderer;

        fallbackPixel = new Texture(renderer, 1, 1);
        renderer.Context.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, new Rgba32(1f, 1f, 1f));
    }

    private ITexture currentTarget = null!;

    public uint TextureHandle => throw new NotImplementedException();

    public Vector2D<int> TextureSize => throw new NotImplementedException();

    public bool HasTransparencies => false;

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        if (renderer.GetBoundTexture(textureUnit) is not TextureAtlas atlas)
        {
            currentTarget = fallbackPixel;
            fallbackPixel.Bind();
            return;
        }

        currentTarget = atlas.WhitePixelRegion;
    }

    public Vector2 ToRegionUV(Vector2 uv) => currentTarget.ToRegionUV(uv);

    private bool disposed;

    public void Dispose()
    {
        if (disposed)
            return;

        fallbackPixel.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TextureWhitePixel(){
        Dispose();
    }
}
