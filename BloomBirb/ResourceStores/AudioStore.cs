using System.Reflection;

namespace BloomBirb.ResourceStores;

public class AudioStore
{
    private static readonly string[] lookup_extensions = new string[]{
        "",
        ".wav",
        ".mp3"
    };

    private Assembly assembly;
    private string prefix;

    public AudioStore(Assembly assembly, string prefix)
    {
        this.assembly = assembly;
        this.prefix = prefix;
    }
}
