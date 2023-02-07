using System.Reflection;
using BloomBirb.Renderers.OpenGL;

namespace BloomBirb.ResourceStores;

public class EmbeddedResourceStore
{
    private Assembly assembly;

    private string prefix = "BloomBirb.Resources";

    public TextureStore Textures { get; private set; }
    public ShaderStore Shaders { get; private set; }
    public AudioStore Audio { get; private set; }

    public EmbeddedResourceStore(OpenGLRenderer renderer)
        : this(renderer, typeof(EmbeddedResourceStore).GetTypeInfo().Assembly)
    {
    }

    public EmbeddedResourceStore(OpenGLRenderer renderer, Assembly assembly)
    {
        this.assembly = assembly;


        Console.WriteLine("Resources available:");
        Console.WriteLine("-------------------");
        foreach (string resource in assembly.GetManifestResourceNames())
        {
            Console.WriteLine(resource);
        }

        Textures = new TextureStore(renderer, assembly, $"{prefix}.Textures");
        Shaders = new ShaderStore(renderer, assembly, $"{prefix}.Shaders");
        Audio = new AudioStore(assembly, $"{prefix}.Audio");
    }

    public Stream? Get(string filename)
        => assembly.GetManifestResourceStream($"{prefix}.{filename}");
}
