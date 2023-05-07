using NAudio.Wave;

namespace BloomFramework.Audio.Formats;

public class WaveAudio : AudioBase
{
    public override int SampleRate => waveReader.WaveFormat.SampleRate;

    private readonly WaveFileReader waveReader;

    public override TimeSpan Time
    {
        get => waveReader.CurrentTime;
        set => waveReader.CurrentTime = value;
    }

    public override TimeSpan Duration => waveReader.TotalTime;

    private readonly int sampleSize;

    // Used to indicate that conversion is necessary for OpenAL use
    private readonly bool shouldConvert;

    public WaveAudio(Stream audioStream)
    {
        waveReader = new WaveFileReader(audioStream);
        sampleSize = waveReader.WaveFormat.BitsPerSample / 8;
        shouldConvert = sampleSize > 2;
        Format = ConvertToBufferFormat(shouldConvert ? 16 : waveReader.WaveFormat.BitsPerSample,
            waveReader.WaveFormat.Channels);
    }

    // A wave file may contain 24 or 32 bit data, which needs to be converted to 16bits
    // This will be used as an intermediate buffer if that is the case.
    private byte[]? fetchBuffer;

    public override void ReadNextSamples(byte[] destinationBuffer)
    {
        int numSamples = destinationBuffer.Length / 2;

        byte[] targetBuffer = destinationBuffer;

        if (shouldConvert)
        {
            if (fetchBuffer is null || fetchBuffer.Length < numSamples * sampleSize)
                fetchBuffer = new byte[numSamples * sampleSize];

            targetBuffer = fetchBuffer;
        }

        int count = waveReader.Read(targetBuffer, 0, targetBuffer.Length);

        if (count < targetBuffer.Length)
        {
            if (Looping)
            {
                while (count < targetBuffer.Length)
                {
                    Time = TimeSpan.Zero;
                    count += waveReader.Read(targetBuffer, count, targetBuffer.Length - count);
                }
            }
            else
            {
                Array.Fill(targetBuffer, (byte)0, count, targetBuffer.Length - count);
            }
        }

        if (!shouldConvert) return;

        if (waveReader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            FloatToPcm16(destinationBuffer, targetBuffer.AsSpan());
        else
            ToPcm16(destinationBuffer, targetBuffer.AsSpan(), sampleSize);
    }

    public override void ReadSamples(byte[] destinationBuffer, int begin)
    {
        waveReader.Position = begin;

        ReadNextSamples(destinationBuffer);
    }

    public override byte[] ReadAllSamples()
    {
        byte[] buffer = new byte[waveReader.Length];

        ReadSamples(buffer, 0);
        return buffer;
    }

    private bool isDisposed;

    protected override void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
            waveReader.Dispose();

        isDisposed = true;
    }
}
