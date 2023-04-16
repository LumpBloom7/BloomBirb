using BloomBirb.Audio.Formats;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public abstract class AudioSource : IDisposable
{
    protected readonly AudioBase Audio;

    protected readonly uint Source;

    private float volume = 1;

    public float Volume
    {
        get => volume;
        set => OpenAl.Al.SetSourceProperty(Source, SourceFloat.Gain, volume = value);
    }

    private float speed = 1;

    public float Speed
    {
        get => speed;
        set => OpenAl.Al.SetSourceProperty(Source, SourceFloat.Pitch, speed = value);
    }

    public abstract TimeSpan Time { get; set; }

    public abstract bool Looping { get; set; }

    public AudioSource(AudioBase audio)
    {
        Audio = audio;
        Source = OpenAl.Al.GenSource();
    }

    public virtual void Play() => OpenAl.Al.SourcePlay(Source);

    public virtual void Stop() => OpenAl.Al.SourceStop(Source);

    public virtual void Pause() => OpenAl.Al.SourcePause(Source);

    protected bool IsDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            Audio.Dispose();
        }

        OpenAl.Al.DeleteSource(Source);
        IsDisposed = true;
    }

    ~AudioSource()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
