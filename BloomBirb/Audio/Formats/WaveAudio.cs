using NAudio.Wave;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class WaveAudio : IAudio
{
    public int SampleRate => waveReader.WaveFormat.SampleRate;

    public BufferFormat Format { get; private set; }

    private WaveFileReader waveReader;

    public TimeSpan Time
    {
        get => waveReader.CurrentTime;
        set => waveReader.CurrentTime = value;
    }

    public bool Looping { get; set; }

    public WaveAudio(Stream audioStream)
    {
        waveReader = new WaveFileReader(audioStream);
        Format = IAudio.ConvertToBufferFormat(waveReader.WaveFormat.BitsPerSample, waveReader.WaveFormat.Channels);
    }

    public void ReadNextSamples(byte[] destinationBuffer)
    {
        int count = waveReader.Read(destinationBuffer, 0, destinationBuffer.Length);

        if (Looping && count < destinationBuffer.Length)
        {
            Time = TimeSpan.Zero;
            waveReader.Read(destinationBuffer, count, destinationBuffer.Length - count);
        }
    }

    public void ReadSamples(byte[] destinationBuffer, int begin)
    {
        waveReader.Position = begin;

        ReadNextSamples(destinationBuffer);
    }

    public byte[] ReadAllSamples()
    {
        byte[] buffer = new byte[waveReader.Length];

        ReadSamples(buffer, 0);
        return buffer;
    }

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
