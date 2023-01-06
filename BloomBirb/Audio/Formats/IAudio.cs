using NAudio.Wave;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public interface IAudio : IDisposable
{
    int SampleRate { get; }

    BufferFormat Format { get; }

    byte[] FetchNext(int count);

    byte[] FetchSamples(int begin, int count);

    byte[] FetchAllSamples();

    protected static BufferFormat ConvertToBufferFormat(int bitsPerSample, int channels)
    {
        if (channels == 1)
        {
            if (bitsPerSample == 8)
                return BufferFormat.Mono8;
            else if (bitsPerSample == 16)
                return BufferFormat.Mono16;
        }
        else if (channels == 2)
        {
            if (bitsPerSample == 8)
                return BufferFormat.Stereo8;
            else if (bitsPerSample == 16)
                return BufferFormat.Stereo16;
        }

        throw new Exception("Wave format not supported.");
    }
}
