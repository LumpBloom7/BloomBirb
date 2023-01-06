using NAudio.Wave;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class WaveAudio : IAudio
{
    public int SampleRate => waveReader.WaveFormat.SampleRate;

    public BufferFormat Format { get; private set; }

    private WaveFileReader waveReader;

    public WaveAudio(Stream audioStream)
    {
        waveReader = new WaveFileReader(audioStream);
        Format = IAudio.ConvertToBufferFormat(waveReader.WaveFormat.BitsPerSample, waveReader.WaveFormat.Channels);
    }

    public byte[] FetchNext(int count)
    {
        byte[] buffer = new byte[count];
        waveReader.Read(buffer, 0, count);

        return buffer;
    }

    public byte[] FetchSamples(int begin, int count)
    {
        waveReader.Position = begin;

        return FetchNext(count);
    }

    public byte[] FetchAllSamples() => FetchSamples(0, (int)waveReader.Length);

    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
            waveReader.Dispose();

        isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
