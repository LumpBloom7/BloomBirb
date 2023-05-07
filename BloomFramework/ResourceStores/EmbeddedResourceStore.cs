using System.Reflection;

namespace BloomFramework.ResourceStores;

public class EmbeddedResourceStore : IResourceStore
{
    private readonly Assembly assembly;

    private readonly string prefix;

    public EmbeddedResourceStore()
        : this(typeof(EmbeddedResourceStore).GetTypeInfo().Assembly, "BloomFramework.Resources")
    {
    }

    public EmbeddedResourceStore(Assembly assembly, string prefix)
    {
        this.assembly = assembly;
        this.prefix = prefix;

        Console.WriteLine("Resources available:");
        Console.WriteLine("-------------------");

        foreach (string resource in assembly.GetManifestResourceNames())
            Console.WriteLine(resource);
    }

    public Stream? Get(string filename)
        => assembly.GetManifestResourceStream($"{prefix}.{filename}");
}
