using System.Reflection;
using BloomBirb.Audio.Format;

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

    public AudioBase Get(string path)
    {
        foreach (string extension in lookup_extensions)
        {
            string fullPath = $"{prefix}.{path}{extension}";
            Stream? stream = assembly.GetManifestResourceStream(fullPath);

            if (stream is null)
                continue;

            if (fullPath.EndsWith(".wav"))
                return new WaveAudio(stream);

            if (fullPath.EndsWith(".mp3"))
                return new MP3Audio(stream);
        }

        throw new FileNotFoundException(path);
    }
}
