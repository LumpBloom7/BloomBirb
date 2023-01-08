using BloomBirb.Audio.Format;
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
        set => OpenAL.AL.SetSourceProperty(Source, SourceFloat.Gain, volume = value);
    }

    private float speed = 1;
    public float Speed
    {
        get => speed;
        set => OpenAL.AL.SetSourceProperty(Source, SourceFloat.Pitch, speed = value);
    }

    public abstract TimeSpan Time { get; set; }

    public abstract bool Looping { get; set; }

    public AudioSource(AudioBase audio)
    {
        Audio = audio;
        Source = OpenAL.AL.GenSource();
    }

    public virtual void Play() => OpenAL.AL.SourcePlay(Source);

    public virtual void Stop() => OpenAL.AL.SourceStop(Source);

    public virtual void Pause() => OpenAL.AL.SourcePause(Source);

    protected bool IsDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            Audio.Dispose();
        }

        OpenAL.AL.DeleteSource(Source);
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
