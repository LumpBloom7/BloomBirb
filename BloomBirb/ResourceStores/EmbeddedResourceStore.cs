using System.Reflection;

namespace BloomBirb.ResourceStores;

public class EmbeddedResourceStore
{
    private Assembly assembly;

    private string prefix = "BloomBirb.Resources";

    public TextureStore Textures { get; private set; }
    public ShaderStore Shaders { get; private set; }
    public AudioStore Audio { get; private set; }

    public EmbeddedResourceStore() : this(typeof(EmbeddedResourceStore).GetTypeInfo().Assembly) { }

    public EmbeddedResourceStore(Assembly assembly)
    {
        this.assembly = assembly;


        Console.WriteLine("Resources available:");
        Console.WriteLine("-------------------");
        foreach (string resource in assembly.GetManifestResourceNames())
        {
            Console.WriteLine(resource);
        }

        Textures = new TextureStore(assembly, $"{prefix}.Textures");
        Shaders = new ShaderStore(assembly, $"{prefix}.Shaders");
        Audio = new AudioStore(assembly, $"{prefix}.Audio");
    }

    public Stream? Get(string filename)
        => assembly.GetManifestResourceStream($"{prefix}.{filename}");
}
