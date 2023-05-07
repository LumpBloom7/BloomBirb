using System.Reflection;

namespace BloomFramework.ResourceStores;

public class EmbeddedResourceStore : IResourceStore
{
    private Assembly assembly;

    private string prefix = "BloomFramework.Resources";

    public EmbeddedResourceStore()
        : this(typeof(EmbeddedResourceStore).GetTypeInfo().Assembly)
    {
    }

    public EmbeddedResourceStore(Assembly assembly)
    {
        this.assembly = assembly;

        Console.WriteLine("Resources available:");
        Console.WriteLine("-------------------");

        foreach (string resource in assembly.GetManifestResourceNames())
            Console.WriteLine(resource);
    }

    public Stream? Get(string filename)
        => assembly.GetManifestResourceStream($"{prefix}.{filename}");
}
