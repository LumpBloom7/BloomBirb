using BloomFramework.Audio.Formats;
using Silk.NET.OpenAL;

namespace BloomFramework.Audio;

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
        buffers = OpenAl.Al.GenBuffers(4);
        freeBuffers = new uint[4];
    }

    private unsafe void bufferLoop()
    {
        while (!stopped)
        {
            OpenAl.Al.GetSourceProperty(Source, GetSourceInteger.BuffersProcessed, out int count);

            if (count == 0)
                continue;

            fixed (uint* freeBuffersPtr = freeBuffers)
                OpenAl.Al.SourceUnqueueBuffers(Source, count, freeBuffersPtr);

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

        int filledBuffers = 0;

        foreach (uint buffer in emptyBuffers)
        {
            if (Audio.Completed)
                break;

            Audio.ReadNextSamples(loadBuffer);
            OpenAl.Al.BufferData(buffer, Audio.Format, loadBuffer, Audio.SampleRate);
            filledBuffers++;
        }

        fixed (uint* emptyBuffersPtr = emptyBuffers)
            OpenAl.Al.SourceQueueBuffers(Source, filledBuffers, emptyBuffersPtr);

        if (Audio.Completed)
            stopped = true;
    }

    public override void Play()
    {
        switch (stopped)
        {
            case false when !paused:
                return;

            case true:
                fillAndQueueBuffers(buffers.AsSpan());

                bufferThread = new Thread(bufferLoop);
                bufferThread.Start();
                break;
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

        OpenAl.Al.DeleteBuffers(buffers);
    }
}
