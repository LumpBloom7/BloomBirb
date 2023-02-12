using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL.Buffers;

namespace BloomBirb.Renderers.OpenGL.Batches;

public abstract class VertexBatch<T> : IVertexBatch<T>, IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    private VertexBuffer<T>[] buffers = null!;

    private int maxBuffers;
    private int availableBuffers;

    private int currentBufferIndex;

    private int bufferSize;

    protected OpenGLRenderer Renderer = null!;

    private bool isInitialized;

    public void Initialize(OpenGLRenderer renderer, int bufferSize, int maxNumberOfBuffers = 100)
    {
        if (isInitialized)
            throw new InvalidOperationException("Vertex batch has already been initialized");

        Renderer = renderer;
        this.bufferSize = bufferSize;
        maxBuffers = maxNumberOfBuffers;
        buffers = new VertexBuffer<T>[maxNumberOfBuffers];

        buffers[0] = CreateVertexBuffer(bufferSize);
        buffers[0].Initialize();
        availableBuffers = 1;

        isInitialized = true;
    }

    public void AddVertex(T vertex)
    {
        VertexBuffer<T> currentBuffer = buffers[currentBufferIndex];

        currentBuffer.AddVertex(ref vertex);

        if (currentBuffer.IsFull)
        {
            currentBuffer.DrawBuffer();
            currentBufferIndex = (currentBufferIndex + 1) % maxBuffers;

            if (currentBufferIndex == availableBuffers)
            {
                if (currentBufferIndex == maxBuffers)
                    throw new InvalidOperationException("Exceeded maximum amount of buffers");

                buffers[currentBufferIndex] = CreateVertexBuffer(bufferSize);
                buffers[currentBufferIndex].Initialize();
                availableBuffers++;
            }
        }
    }

    public void FlushBatch()
    {
        if (buffers[currentBufferIndex].Count > 0)
            buffers[currentBufferIndex].DrawBuffer();

        currentBufferIndex = 0;
    }

    protected abstract VertexBuffer<T> CreateVertexBuffer(int size);

    public void Dispose()
    {
        foreach (VertexBuffer<T> buffer in buffers)
            buffer?.Dispose();

        GC.SuppressFinalize(this);
    }
}
