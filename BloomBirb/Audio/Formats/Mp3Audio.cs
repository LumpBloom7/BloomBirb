using NLayer;

namespace BloomBirb.Audio.Formats;

public class Mp3Audio : AudioBase
{
    public override int SampleRate => mpegReader.SampleRate;

    private readonly MpegFile mpegReader;

    public override TimeSpan Time
    {
        get => mpegReader.Time;
        set => mpegReader.Time = value;
    }

    public Mp3Audio(Stream audioStream)
    {
        mpegReader = new MpegFile(audioStream);
        Format = ConvertToBufferFormat(16, mpegReader.Channels);
    }

    public override byte[] ReadAllSamples()
    {
        byte[] buffer = new byte[mpegReader.Length / sizeof(float)];

        ReadSamples(buffer, 0);

        return buffer;
    }

    // Used because mpegFileRead provides a byte[] with Float32 data, while we need int16 data
    // This will be used as an intermediate buffer.
    private byte[]? fetchBuffer;

    public override void ReadNextSamples(byte[] destinationBuffer)
    {
        int numSamples = destinationBuffer.Length / 2;
        int numBytes = numSamples * sizeof(float);

        if (fetchBuffer is null || fetchBuffer.Length < numBytes)
            fetchBuffer = new byte[numBytes];

        int count = mpegReader.ReadSamples(fetchBuffer, 0, numBytes);

        // Implement loopback
        while (Looping && count < numBytes)
        {
            Time = TimeSpan.Zero;
            count += mpegReader.ReadSamples(fetchBuffer, count, numBytes - count);
        }

        FloatToPcm16(destinationBuffer, fetchBuffer.AsSpan(0, numBytes));
    }

    public override void ReadSamples(byte[] destinationBuffer, int begin)
    {
        mpegReader.Position = begin;
        ReadNextSamples(destinationBuffer);
    }

    private bool isDisposed;

    protected override void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
            mpegReader.Dispose();

        isDisposed = true;
    }
}
