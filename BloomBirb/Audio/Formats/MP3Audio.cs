using NLayer;

namespace BloomBirb.Audio.Format;

public class MP3Audio : AudioBase
{
    public override int SampleRate => mpegReader.SampleRate;

    private MpegFile mpegReader;

    public override TimeSpan Time
    {
        get => mpegReader.Time;
        set => mpegReader.Time = value;
    }

    public MP3Audio(Stream audioStream)
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
        int numFloats = destinationBuffer.Length / 2;

        if (fetchBuffer is null || fetchBuffer.Length < numFloats * sizeof(float))
            fetchBuffer = new byte[numFloats * sizeof(float)];

        int count = mpegReader.ReadSamples(fetchBuffer, 0, numFloats);

        // Implement loopback
        if (Looping && count < numFloats)
        {
            Time = TimeSpan.Zero;
            mpegReader.ReadSamples(fetchBuffer, count * sizeof(float), numFloats - count);
        }

        FloatToPCM16(destinationBuffer, fetchBuffer.AsSpan(0, numFloats * sizeof(float)));
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
