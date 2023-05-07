using NLayer;

namespace BloomFramework.Audio.Formats;

public class Mp3Audio : AudioBase
{
    public override int SampleRate => mpegReader.SampleRate;

    private readonly MpegFile mpegReader;

    public override TimeSpan Time
    {
        get => mpegReader.Time;
        set
        {
            mpegReader.Time = value;
            completed = value >= Duration;
        }
    }

    // There's a precision error within mpegReader.Time setter
    // So we must determine completion ourselves
    private bool completed;
    public override bool Completed => completed;

    public override TimeSpan Duration => mpegReader.Duration;

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

        // Reached end of file
        if (count < numBytes)
        {
            if (Looping) // Loop if needed
            {
                while (count < numBytes)
                {
                    Time = TimeSpan.Zero;
                    count += mpegReader.ReadSamples(fetchBuffer, count, numBytes - count);
                }
            }
            else
            {
                completed = true;
                Array.Fill(fetchBuffer, (byte)0, count, numBytes - count);
            }
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
