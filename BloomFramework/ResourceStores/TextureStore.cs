using System.Diagnostics;
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

    private readonly Dictionary<string, ITexture> textureCache = new();

    private readonly IResourceStore resources;

    private readonly string prefix;

    private readonly OpenGlRenderer renderer;

    private readonly TextureParameters textureParameters;

    public TextureStore(OpenGlRenderer renderer, IResourceStore resourceStore, string prefix = "Textures")
        : this(renderer, resourceStore, new TextureParameters(), prefix)
    {
    }

    public TextureStore(OpenGlRenderer renderer, IResourceStore resourceStore, TextureParameters parameters,
        string prefix = "Textures")
    {
        this.renderer = renderer;
        resources = resourceStore;
        this.prefix = prefix;
        textureParameters = parameters;
    }

    public ITexture Get(string filename)
    {
        foreach (string fallbackExt in lookup_extensions)
        {
            string actualFilename = $"{filename}{fallbackExt}";
            if (textureCache.TryGetValue(actualFilename, out var texture)) return texture;

            var stream = resources.Get($"{prefix}.{actualFilename}");

            if (stream is null)
                continue;

            ITexture newTexture;

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

        return renderer.BlankTexture;
    }

    private ITexture addLargeTexture(Image<Rgba32> image)
    {
        var texture = new Texture(renderer, image.Width, image.Height, textureParameters);
        var usage = texture.UploadData(image);

        textures.Add(texture);
        return usage;
    }

    private ITexture addRegularTexture(Image<Rgba32> image)
    {
        foreach (var textureAtlas in atlases)
        {
            if (textureAtlas.TryAddSubtexture(image, out ITexture? textureUsage))
                return textureUsage;
        }

        // No fitting atlas, create new
        var newAtlas = new TextureAtlas(renderer, 4096, 4096, textureParameters);

        atlases.Add(newAtlas);

        newAtlas.TryAddSubtexture(image, out ITexture? textureUsage2);

        Debug.Assert(textureUsage2 is not null);

        return textureUsage2;
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
