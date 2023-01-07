using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class AudioStreamSource : IDisposable
{
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

    public TimeSpan Time
    {
        get => audio.Time;
        set => audio.Time = value;
    }

    private bool looping = false;

    public bool Looping
    {
        get => audio.Looping;
        set => audio.Looping = value;
    }

    private readonly IAudio audio;

    private uint source;

    private uint[] buffers;
    private uint[] unqueuedBuffers;
    private int numberOfFreeBuffers;

    private Thread bufferThread;

    private bool threadShutDown;

    public AudioStreamSource(IAudio audio)
    {
        this.audio = audio;

        source = OpenAL.AL.GenSource();
        buffers = OpenAL.AL.GenBuffers(4);

        unqueuedBuffers = new uint[4];
        Array.Copy(buffers, unqueuedBuffers, 4);

        numberOfFreeBuffers = 4;

        bufferData();

        bufferThread = new Thread(() =>
        {
            while (!threadShutDown)
            {
                OpenAL.AL.GetSourceProperty(source, GetSourceInteger.BuffersProcessed, out int freeBuffers);

                if (freeBuffers == 0)
                    continue;

                uint[] tmpBuff = new uint[freeBuffers];

                OpenAL.AL.SourceUnqueueBuffers(source, tmpBuff);

                unqueuedBuffers = tmpBuff;

                numberOfFreeBuffers = freeBuffers;

                bufferData();
            }
        });

        bufferThread.Start();
    }

    // Reusable temporary buffer to buffer stuff in
    // Reduces gc strain due to the large counts
    private byte[] loadBuffer = new byte[8192];

    private void bufferData()
    {
        if (numberOfFreeBuffers == 0)
            return;

        for (int i = 0; i < numberOfFreeBuffers; ++i)
        {
            audio.ReadNextSamples(loadBuffer);
            OpenAL.AL.BufferData(unqueuedBuffers[i], audio.Format, loadBuffer, audio.SampleRate);
        }

        OpenAL.AL.SourceQueueBuffers(source, unqueuedBuffers);
        numberOfFreeBuffers = 0;
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

        threadShutDown = true;
        bufferThread.Join();

        OpenAL.AL.DeleteSource(source);
        OpenAL.AL.DeleteBuffers(buffers);

        isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~AudioStreamSource()
    {
        Dispose(disposing: false);
    }
}
