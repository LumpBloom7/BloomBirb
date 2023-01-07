using NLayer;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class MP3Audio : IAudio
{
    public int SampleRate => mpegReader.SampleRate;

    public BufferFormat Format { get; private set; }

    private MpegFile mpegReader;

    public MP3Audio(Stream audioStream)
    {
        mpegReader = new MpegFile(audioStream);
        Format = IAudio.ConvertToBufferFormat(16, mpegReader.Channels);
    }

    public byte[] ReadAllSamples()
    {
        byte[] buffer = new byte[mpegReader.Length / sizeof(float)];

        ReadSamples(buffer, 0);

        return buffer;
    }

    // Used because mpegFileRead provides a byte[] with Float32 data, while we need int16 data
    // This will be used as an intermediate buffer.
    private float[]? fetchBuffer;

    public void ReadNextSamples(byte[] destinationBuffer)
    {
        int numFloats = destinationBuffer.Length / 2;

        if (fetchBuffer is null || fetchBuffer.Length < numFloats)
            fetchBuffer = new float[numFloats];

        mpegReader.ReadSamples(fetchBuffer, 0, numFloats);

        floatToPCM16(destinationBuffer, fetchBuffer.AsSpan(0, numFloats));
    }

    public void ReadSamples(byte[] destinationBuffer, int begin)
    {
        mpegReader.Position = begin;
        ReadNextSamples(destinationBuffer);
    }

    private void floatToPCM16(byte[] destination, Span<float> floatSamples)
    {
        for (int i = 0; i < floatSamples.Length; ++i)
        {
            float floatValue = normalizeFloat(floatSamples[i]);
            short sampleValue = (short)(floatValue * short.MaxValue);

            destination[i * sizeof(short)] = (byte)sampleValue;
            destination[(i * sizeof(short)) + 1] = (byte)(sampleValue >> 8);
        }
    }

    private float maxFloatDivisor = 1;
    private float floatNormalizeFactor = 1f;

    // This is used to "fix" some MP3 files with out of range samples
    private float normalizeFloat(float value)
    {
        float maxRange = Math.Abs(value);
        if (maxFloatDivisor < maxRange)
        {
            maxFloatDivisor = maxRange;
            floatNormalizeFactor = 1 / maxFloatDivisor;
        }

        return value * floatNormalizeFactor;
    }

    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
            mpegReader.Dispose();

        isDisposed = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
