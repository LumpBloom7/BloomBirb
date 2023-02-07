using BloomBirb.Audio.Format;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class SoundSource : AudioSource
{
    public override TimeSpan Time
    {
        get
        {
            OpenAL.AL.GetSourceProperty(Source, SourceFloat.SecOffset, out float seconds);
            return TimeSpan.FromSeconds(seconds);
        }
        set => OpenAL.AL.SetSourceProperty(Source, SourceFloat.SecOffset, (float)value.TotalSeconds);
    }

    private bool looping;

    public override bool Looping
    {
        get => looping;
        set
        {
            if (looping != value)
                looping = value;

            OpenAL.AL.SetSourceProperty(Source, SourceBoolean.Looping, looping);
        }
    }

    private readonly uint buffer;

    public SoundSource(AudioBase audio) : base(audio)
    {
        buffer = OpenAL.AL.GenBuffer();

        byte[] audioData = audio.ReadAllSamples();

        OpenAL.AL.BufferData(buffer, audio.Format, audioData, audio.SampleRate);
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        base.Dispose(disposing);
        OpenAL.AL.DeleteBuffer(buffer);
    }
}
