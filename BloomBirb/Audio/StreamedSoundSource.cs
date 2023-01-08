using BloomBirb.Audio.Format;
using Silk.NET.OpenAL;

namespace BloomBirb.Audio;

public class StreamedSoundSource : AudioSource
{
    public override TimeSpan Time
    {
        get => Audio.Time;
        set => Audio.Time = value;
    }

    public override bool Looping
    {
        get => Audio.Looping;
        set => Audio.Looping = value;
    }

    private readonly uint[] buffers;
    private readonly uint[] freeBuffers;

    private Thread? bufferThread;

    private bool stopped = true;
    private bool paused;

    public StreamedSoundSource(AudioBase audio) : base(audio)
    {
        buffers = OpenAL.AL.GenBuffers(4);
        freeBuffers = new uint[4];
    }

    private unsafe void bufferLoop()
    {
        while (!stopped)
        {
            OpenAL.AL.GetSourceProperty(Source, GetSourceInteger.BuffersProcessed, out int count);

            if (count == 0)
                continue;

            fixed (uint* freeBuffers_ = freeBuffers)
                OpenAL.AL.SourceUnqueueBuffers(Source, count, freeBuffers_);

            fillAndQueueBuffers(freeBuffers.AsSpan(0, count));
        }
    }

    // Reusable temporary buffer to buffer stuff in
    // Reduces gc strain due to the large buffers
    private byte[] loadBuffer = new byte[8192];

    private unsafe void fillAndQueueBuffers(Span<uint> emptyBuffers)
    {
        if (emptyBuffers.IsEmpty)
            return;

        for (int i = 0; i < emptyBuffers.Length; ++i)
        {
            Audio.ReadNextSamples(loadBuffer);
            OpenAL.AL.BufferData(emptyBuffers[i], Audio.Format, loadBuffer, Audio.SampleRate);
        }

        fixed (uint* filledBuffers = emptyBuffers)
            OpenAL.AL.SourceQueueBuffers(Source, emptyBuffers.Length, filledBuffers);
    }

    public override void Play()
    {
        if (!stopped && !paused)
            return;

        if (stopped)
        {
            fillAndQueueBuffers(buffers.AsSpan());

            bufferThread = new Thread(bufferLoop);
            bufferThread.Start();
        }

        stopped = paused = false;

        base.Play();
    }
    public override void Pause()
    {
        paused = true;

        base.Pause();
    }

    public override void Stop()
    {
        base.Stop();

        stopped = true;
        bufferThread?.Join();

        Time = TimeSpan.Zero;
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        Stop();
        base.Dispose(disposing);

        OpenAL.AL.DeleteBuffers(buffers);
    }
}
