using NAudio.Wave;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public interface IAudio : IDisposable
{
    int SampleRate { get; }

    BufferFormat Format { get; }

    TimeSpan Time { get; set; }

    bool Looping { get; set; }

    /// <summary>
    /// Fills the destination buffer with sample data, starting from the current read position.
    ///
    /// The number of samples is determined by the length of the provided buffer.
    /// </summary>
    /// <param name="destinationBuffer">A byte array to buffer data into.</param>
    void ReadNextSamples(byte[] destinationBuffer);

    /// <summary>
    /// Fills the destination buffer with sample data, starting from a specified position.
    ///
    /// The number of samples is determined by the length of the provided buffer.
    /// </summary>
    /// <param name="destinationBuffer">A byte array to buffer data into.</param>
    /// <param name="begin">The position to start the read from.</param>
    void ReadSamples(byte[] destinationBuffer, int begin);

    /// <summary>
    /// Reads all sample data contained within the audio stream.
    /// </summary>
    /// <returns>A `byte[]` containing sample data </returns>
    byte[] ReadAllSamples();

    protected static BufferFormat ConvertToBufferFormat(int bitsPerSample, int channels)
    {
        if (channels == 1)
        {
            if (bitsPerSample == 8)
                return BufferFormat.Mono8;
            else if (bitsPerSample == 16)
                return BufferFormat.Mono16;
        }
        else if (channels == 2)
        {
            if (bitsPerSample == 8)
                return BufferFormat.Stereo8;
            else if (bitsPerSample == 16)
                return BufferFormat.Stereo16;
        }

        throw new Exception("Wave format not supported.");
    }
}
