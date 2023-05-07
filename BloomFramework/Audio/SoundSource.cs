using BloomFramework.Audio.Formats;
using Silk.NET.OpenAL;

namespace BloomFramework.Audio;

public class SoundSource : AudioSource
{
    public override TimeSpan Time
    {
        get
        {
            OpenAl.Al.GetSourceProperty(Source, SourceFloat.SecOffset, out float seconds);
            return TimeSpan.FromSeconds(seconds);
        }
        set => OpenAl.Al.SetSourceProperty(Source, SourceFloat.SecOffset, (float)value.TotalSeconds);
    }

    private bool looping;

    public override bool Looping
    {
        get => looping;
        set
        {
            if (looping == value)
                return;

            looping = value;
            OpenAl.Al.SetSourceProperty(Source, SourceBoolean.Looping, looping);
        }
    }

    private readonly uint buffer;

    public SoundSource(AudioBase audio) : base(audio)
    {
        buffer = OpenAl.Al.GenBuffer();

        byte[] audioData = audio.ReadAllSamples();

        OpenAl.Al.BufferData(buffer, audio.Format, audioData, audio.SampleRate);
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        base.Dispose(disposing);
        OpenAl.Al.DeleteBuffer(buffer);
    }
}
