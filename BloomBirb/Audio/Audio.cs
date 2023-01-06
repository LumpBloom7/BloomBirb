using NAudio.Wave;
using NLayer;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class Audio
{
    public uint OpenALBuffer { get; private set; }

    public unsafe Audio(byte[] data, int channels, int bitRate, int sampleRate)
    {
        var BufferFormat = convertToBufferFormat(bitRate, channels);
        OpenALBuffer = OpenAL.AL.GenBuffer();

        fixed (byte* pData = data)
            OpenAL.AL.BufferData(OpenALBuffer, BufferFormat, pData, data.Length, sampleRate);
    }

    public static Audio LoadFromWAV(Stream wavStream)
    {
        using (WaveFileReader? wreader = new WaveFileReader(wavStream))
        {
            byte[] waveBuffer = new byte[wreader.Length];
            wreader.Read(waveBuffer, 0, (int)wreader.Length);
            var form = wreader.WaveFormat;

            return new Audio(waveBuffer, form.Channels, form.BitsPerSample, form.SampleRate);
        }
    }

    public static Audio LoadFromMP3(Stream mp3Stream)
    {
        var mpegFile = new MpegFile(mp3Stream);
        int sampleNumber = (int)mpegFile.Length / sizeof(float);
        float[] samples = new float[sampleNumber];
        mpegFile.ReadSamples(samples, 0, sampleNumber);

        byte[]? pcmSamples = floatToPCM16(samples);

        return new Audio(pcmSamples, mpegFile.Channels, 16, mpegFile.SampleRate);
    }

    private static byte[] floatToPCM16(float[] floatSamples)
    {
        byte[] pcmSamples = new byte[floatSamples.Length * sizeof(short)];

        for (int i = 0; i < floatSamples.Length; ++i)
        {
            short sampleValue = (short)(Math.Clamp(floatSamples[i], -1, 1) * short.MaxValue);

            byte[] bytes = BitConverter.GetBytes(sampleValue);
            pcmSamples[i * sizeof(short)] = bytes[0];
            pcmSamples[i * sizeof(short) + 1] = bytes[1];
        }

        return pcmSamples;
    }

    private static BufferFormat convertToBufferFormat(int bitsPerSample, int channels)
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
