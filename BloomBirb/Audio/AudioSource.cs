using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class AudioSource : IDisposable
{
    private readonly IAudio audio;

    private uint source;

    private uint buffer;

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

    private bool looping = false;

    public bool Looping
    {
        get => looping;
        set
        {
            looping = value;
            OpenAL.AL.SetSourceProperty(source, SourceBoolean.Looping, value);
        }
    }

    public AudioSource(IAudio audio)
    {
        this.audio = audio;

        source = OpenAL.AL.GenSource();
        buffer = OpenAL.AL.GenBuffer();

        // Buffer in audio data
        byte[]? data = audio.FetchAllSamples();
        OpenAL.AL.BufferData(buffer, audio.Format, data, audio.SampleRate);

        OpenAL.AL.SetSourceProperty(source, SourceInteger.Buffer, buffer);
    }

    public void Play() => OpenAL.AL.SourcePlay(source);

    public void Stop() => OpenAL.AL.SourceStop(source);

    public void Pause() => OpenAL.AL.SourcePause(source);

    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        // TODO: We may not want to eagerly dispose this
        if (disposing)
            audio.Dispose();


        OpenAL.AL.DeleteSource(source);
        OpenAL.AL.DeleteBuffer(buffer);

        isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~AudioSource()
    {
        Dispose(disposing: false);
    }
}
