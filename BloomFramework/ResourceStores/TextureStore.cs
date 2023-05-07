using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BloomFramework.ResourceStores;

public class TextureStore : IDisposable
{
    private static readonly string[] lookup_extensions =
    {
        "", // In case the user already specified an extension
        ".png",
        ".jpg",
        ".bmp"
    };

    private readonly List<TextureAtlas> atlases = new();
    private readonly List<Texture> textures = new();

    private readonly Dictionary<string, TextureUsage> textureCache = new();

    private readonly IResourceStore resources;

    private readonly string prefix;

    private readonly OpenGlRenderer renderer;

    private readonly int mipMapLevels;

    public TextureStore(OpenGlRenderer renderer, IResourceStore resourceStore, string prefix = "Textures", int mipLevels = 4)
    {
        this.renderer = renderer;
        resources = resourceStore;
        this.prefix = prefix;
        mipMapLevels = mipLevels;
    }

    public TextureUsage Get(string filename)
    {
        foreach (string fallbackExt in lookup_extensions)
        {
            string actualFilename = $"{filename}{fallbackExt}";
            if (!textureCache.TryGetValue(actualFilename, out var texture))
            {
                var stream = resources.Get($"{prefix}.{actualFilename}");

                if (stream is null)
                    continue;

                TextureUsage newTexture;

                using (var image = Image.Load<Rgba32>(stream))
                {
                    if (image.Width >= 2048 || image.Height >= 2048)
                        newTexture = addLargeTexture(image);
                    else
                        newTexture = addRegularTexture(image);

                }

                textureCache.Add(actualFilename, newTexture);
                return newTexture;
            }

            return texture;
        }

        return renderer.BlankTexture;
    }

    private TextureUsage addLargeTexture(Image<Rgba32> image)
    {
        var texture = new Texture(renderer, mipMapLevels);
        texture.Initialize(image.Size());

        texture.BufferImageData(image);

        textures.Add(texture);
        return texture;
    }

    private TextureUsage addRegularTexture(Image<Rgba32> image)
    {
        foreach (var textureAtlas in atlases)
        {
            var textureUsage = textureAtlas.AddSubtexture(image);
            if (textureUsage is not null)
                return textureUsage;
        }

        // No fitting atlas, create new
        var newAtlas = new TextureAtlas(renderer, mipMapLevels);
        newAtlas.Initialize(new Size(4096, 4096));

        atlases.Add(newAtlas);

        return newAtlas.AddSubtexture(image)!;
    }

    private bool isDisposed;

    protected void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            foreach (var texture in textures)
                texture.Dispose();

            foreach (var atlas in atlases)
                atlas.Dispose();

            textureCache.Clear();
        }

        isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
