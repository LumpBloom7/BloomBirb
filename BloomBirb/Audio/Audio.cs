using NAudio.Wave;
using NLayer;
using NLayer.NAudioSupport;
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
            else if (bitsPerSample == 32)
                return BufferFormat.Stereo16;
        }

        throw new Exception("Wave format not supported.");
    }
}
