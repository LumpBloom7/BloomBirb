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
            Stream? stream = assembly.GetManifestResourceStream($"{prefix}.{path}{extension}");

            if (stream is null)
                continue;

            try
            {
                return new WaveAudio(stream);
            }
            catch (FormatException) { }

            try
            {
                return new MP3Audio(stream);
            }
            catch (FormatException)
            {
                throw new FormatException($"{path} is not a WAV or an MP3 file");
            }
        }

        throw new FileNotFoundException(path);
    }
}
