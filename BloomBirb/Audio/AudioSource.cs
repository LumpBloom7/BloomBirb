using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class AudioSource
{
    private readonly Audio audio;

    private uint source;

    private float volume = 1;
    public float Volume
    {
        get => volume;
        set
        {
            volume = value;
            OpenAL.AL.SetSourceProperty(source, SourceFloat.Gain, value);
        }
    }

    private float pitch = 1;
    public float Pitch
    {
        get => pitch;
        set
        {
            pitch = value;
            OpenAL.AL.SetSourceProperty(source, SourceFloat.Pitch, value);
        }
    }

    public float Elapsed
    {
        get
        {
            OpenAL.AL.GetSourceProperty(source, SourceFloat.SecOffset, out float offset);
            return offset;
        }
        set => OpenAL.AL.SetSourceProperty(source, SourceFloat.SecOffset, value);
    }


    public AudioSource(Audio audio)
    {
        this.audio = audio;

        source = OpenAL.AL.GenSource();
        OpenAL.AL.SetSourceProperty(source, SourceInteger.Buffer, audio.OpenALBuffer);
    }

    public void Play() => OpenAL.AL.SourcePlay(source);

    public void Stop() => OpenAL.AL.SourceStop(source);

    public void Pause() => OpenAL.AL.SourcePause(source);
}
