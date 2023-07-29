using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Textures;

public readonly record struct TextureParameters()
{
    public static readonly TextureParameters PIXEL_ART = new ()
    {
        TextureMinFilter = TextureMinFilter.NearestMipmapNearest,
        TextureMagFilter = TextureMagFilter.Nearest
    };

    public TextureWrapMode TextureWrapX { get; init; } = TextureWrapMode.ClampToEdge;
    public TextureWrapMode TextureWrapY { get; init; } = TextureWrapMode.ClampToEdge;
    public TextureWrapMode TextureWrapZ { get; init; } = TextureWrapMode.ClampToEdge;

    public TextureMinFilter TextureMinFilter { get; init; } = TextureMinFilter.LinearMipmapLinear;
    public TextureMagFilter TextureMagFilter { get; init; } = TextureMagFilter.Linear;

    public int BaseMipLevel { get; init; } = 0;
    public int MaxMipLevel { get; init; } = 4;
}
