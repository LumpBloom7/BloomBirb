using NAudio.Wave;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio.Formats;

public abstract class AudioBase : IDisposable
{
    public abstract int SampleRate { get; }

    public BufferFormat Format { get; protected init; }

    public abstract TimeSpan Time { get; set; }

    public bool Looping { get; set; }

    /// <summary>
    /// Fills the destination buffer with sample data, starting from the current read position.
    ///
    /// The number of samples is determined by the length of the provided buffer.
    /// </summary>
    /// <param name="destinationBuffer">A byte array to buffer data into.</param>
    public abstract void ReadNextSamples(byte[] destinationBuffer);

    /// <summary>
    /// Fills the destination buffer with sample data, starting from a specified position.
    ///
    /// The number of samples is determined by the length of the provided buffer.
    /// </summary>
    /// <param name="destinationBuffer">A byte array to buffer data into.</param>
    /// <param name="begin">The position to start the read from.</param>
    public abstract void ReadSamples(byte[] destinationBuffer, int begin);

    /// <summary>
    /// Reads all sample data contained within the audio stream.
    /// </summary>
    /// <returns>A `byte[]` containing sample data </returns>
    public abstract byte[] ReadAllSamples();

    protected static void ThrowIfInvalid(WaveFormat format)
    {
        switch (format.Encoding)
        {
            case WaveFormatEncoding.Pcm:
            case WaveFormatEncoding.IeeeFloat:
                return;
        }

        throw new InvalidDataException("Only IEEEFloat or PCM encoding supported");
    }

    protected static BufferFormat ConvertToBufferFormat(int bitsPerSample, int channels)
    {
        if (channels == 1)
        {
            if (bitsPerSample == 8)
                return BufferFormat.Mono8;

            if (bitsPerSample == 16)
                return BufferFormat.Mono16;
        }
        else if (channels == 2)
        {
            if (bitsPerSample == 8)
                return BufferFormat.Stereo8;

            if (bitsPerSample == 16)
                return BufferFormat.Stereo16;
        }

        throw new Exception("Wave format not supported.");
    }

    protected static void ToPcm16(byte[] destination, Span<byte> pcmSamples, int sampleSize)
    {
        for (int i = 0; i < pcmSamples.Length / sampleSize; ++i)
        {
            int value = 0;

            for (int j = 0; j < sampleSize; ++j)
                value += pcmSamples[i * sampleSize + j] << (8 * j);

            if (sampleSize == 3)
                value <<= 8;

            short sampleValue = (short)(value / (float)int.MaxValue * short.MaxValue);

            destination[i * sizeof(short)] = (byte)sampleValue;
            destination[(i * sizeof(short)) + 1] = (byte)(sampleValue >> 8);
        }
    }

    protected void FloatToPcm16(byte[] destination, Span<byte> floatSamples)
    {
        for (int i = 0; i < floatSamples.Length / sizeof(float); ++i)
        {
            float floatValue =
                normalizeFloat(BitConverter.ToSingle(floatSamples.Slice(i * sizeof(float), sizeof(float))));
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

        if (maxFloatDivisor >= maxRange)
            return value * floatNormalizeFactor;

        maxFloatDivisor = maxRange;
        floatNormalizeFactor = 1 / maxFloatDivisor;

        return value * floatNormalizeFactor;
    }


    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
