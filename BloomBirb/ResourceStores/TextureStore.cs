using System.Reflection;
using BloomBirb.Renderers.OpenGL;

namespace BloomBirb.ResourceStores;

public class TextureStore
{
    private static readonly string[] lookup_extensions =
    {
        "", // In case the user already specified an extension
        ".png",
        ".jpg",
        ".bmp"
    };

    private readonly Dictionary<string, Texture> textureCache = new();

    private readonly Assembly assembly;

    private readonly string prefix;

    private readonly OpenGLRenderer renderer;

    public TextureStore(OpenGLRenderer renderer, Assembly assembly, string prefix)
    {
        this.renderer = renderer;
        this.assembly = assembly;
        this.prefix = prefix;
    }

    public Texture Get(string filename)
    {
        foreach (string fallbackExt in lookup_extensions)
        {
            string actualFilename = $"{filename}{fallbackExt}";
            if (!textureCache.TryGetValue(actualFilename, out var texture))
            {
                var stream = assembly.GetManifestResourceStream($"{prefix}.{actualFilename}");
                if (stream is null)
                    continue;

                var newTexture = renderer.CreateTexture(stream);
                textureCache.Add(actualFilename, newTexture);
                return newTexture;
            }

            return texture;
        }

        return renderer.BlankTexture!;
    }
}
