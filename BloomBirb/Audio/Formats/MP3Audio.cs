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

    public byte[] FetchAllSamples() => FetchSamples(0, (int)(mpegReader.Length / sizeof(float)));

    public byte[] FetchNext(int count)
    {
        float[] floatSamples = new float[count];
        mpegReader.ReadSamples(floatSamples, 0, count);

        normalizeFloats(floatSamples);

        byte[] byteBuffer = floatToPCM16(floatSamples);
        return byteBuffer;
    }

    public byte[] FetchSamples(int begin, int count)
    {
        mpegReader.Position = begin;
        return FetchNext(count);
    }


    private static byte[] floatToPCM16(float[] floatSamples)
    {
        byte[] pcmSamples = new byte[floatSamples.Length * sizeof(short)];

        for (int i = 0; i < floatSamples.Length; ++i)
        {
            short sampleValue = (short)(floatSamples[i] * short.MaxValue);

            byte[] bytes = BitConverter.GetBytes(sampleValue);
            pcmSamples[i * sizeof(short)] = bytes[0];
            pcmSamples[i * sizeof(short) + 1] = bytes[1];
        }

        return pcmSamples;
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
