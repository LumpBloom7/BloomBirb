using BloomFramework.Audio.Formats;

namespace BloomFramework.ResourceStores;

public class AudioStore
{
    private static readonly string[] lookup_extensions = {
        "",
        ".wav",
        ".mp3"
    };

    private readonly IResourceStore resources;
    private readonly string prefix;

    public AudioStore(IResourceStore resources, string prefix = "Audio")
    {
        this.resources = resources;
        this.prefix = prefix;
    }

    public AudioBase Get(string path)
    {
        foreach (string extension in lookup_extensions)
        {
            string fullPath = $"{prefix}.{path}{extension}";
            Stream? stream = resources.Get(fullPath);

            if (stream is null)
                continue;

            if (fullPath.EndsWith(".wav"))
                return new WaveAudio(stream);

            if (fullPath.EndsWith(".mp3"))
                return new Mp3Audio(stream);
        }

        throw new FileNotFoundException(path);
    }
}
