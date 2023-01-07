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

    public void ReadNextSamples(byte[] destinationBuffer)
    {
        int numFloats = destinationBuffer.Length / 2;
        float[] floatSamples = new float[numFloats];
        mpegReader.ReadSamples(floatSamples, 0, numFloats);

        normalizeFloats(floatSamples);

        floatToPCM16(destinationBuffer, floatSamples);
    }

    public void ReadSamples(byte[] destinationBuffer, int begin)
    {
        mpegReader.Position = begin;
        ReadNextSamples(destinationBuffer);
    }


    private static void floatToPCM16(byte[] destination, float[] floatSamples)
    {
        for (int i = 0; i < floatSamples.Length; ++i)
        {
            short sampleValue = (short)(floatSamples[i] * short.MaxValue);

            destination[i * sizeof(short)] = (byte)sampleValue;
            destination[(i * sizeof(short)) + 1] = (byte)(sampleValue >> 8);
        }
    }

    private float maxFloatRange = 1f;

    // This is used to "fix" some MP3 files with out of range samples
    private void normalizeFloats(float[] floatSamples)
    {
        maxFloatRange = Math.Max(maxFloatRange, floatSamples.Max(Math.Abs));

        if (maxFloatRange <= 1f)
            return;

        for (int i = 0; i < floatSamples.Length; ++i)
            floatSamples[i] /= maxFloatRange;
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
