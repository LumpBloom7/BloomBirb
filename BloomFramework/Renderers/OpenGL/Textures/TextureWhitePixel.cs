using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomFramework.Renderers.OpenGL.Textures;

public class TextureWhitePixel : ITextureUsage
{
    // Intentionally null due to the dynamic nature
    public ITexture BackingTexture => null!;
    public bool HasTransparencies => false;
    public Vector2 RegionOrigin { get; } = Vector2.Zero;
    public Vector2 RegionSize { get; private set; }

    private Vector2D<int> backingTextureSize = new (1, 1);

    private readonly OpenGlRenderer renderer;

    public TextureWhitePixel(OpenGlRenderer renderer)
    {
        this.renderer = renderer;
    }

    private Texture? fallbackTexture;

    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        var currentlyBoundTexture = renderer.GetBoundTexture(textureUnit);

        if (currentlyBoundTexture is not TextureAtlas)
        {
            currentlyBoundTexture = fallbackTexture ??= createFallbackTexture();
            currentlyBoundTexture.Bind(textureUnit);
        }

        if (backingTextureSize.Equals(currentlyBoundTexture.TextureSize))
            return;

        backingTextureSize = currentlyBoundTexture.TextureSize;
        RegionSize = new Vector2(1f / backingTextureSize.X, 1f / backingTextureSize.Y);
    }

    private Texture createFallbackTexture()
    {
        var texture = new Texture(renderer, 1, 1);
        renderer.Context.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 1, 1, PixelFormat.Rgba,
            PixelType.UnsignedByte, new Rgba32(1f, 1f, 1f));

        return texture;
    }
}
