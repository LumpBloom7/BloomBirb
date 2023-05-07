using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL.Buffers;

namespace BloomFramework.Renderers.OpenGL.Batches;

public abstract class VertexBatch<T> : IVertexBatch<T>, IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    [MemberNotNullWhen(true, nameof(isInitialized))]
    private VertexBuffer<T>?[] buffers { get; set; } = null!;

    private VertexBuffer<T> currentBuffer = null!;

    private int maxBuffers;

    private int currentBufferIndex;

    private int size;

    protected OpenGlRenderer Renderer = null!;

    private bool isInitialized;

    public void Initialize(OpenGlRenderer renderer, int bufferSize, int maxNumberOfBuffers = 100)
    {
        if (isInitialized)
            throw new InvalidOperationException("Vertex batch has already been initialized");

        Renderer = renderer;
        size = bufferSize;
        maxBuffers = maxNumberOfBuffers;
        buffers = new VertexBuffer<T>[maxNumberOfBuffers];

        buffers[0] = currentBuffer = CreateVertexBuffer(bufferSize);
        currentBuffer.Initialize();

        isInitialized = true;
    }

    public void AddVertex(T vertex)
    {
        Debug.Assert(isInitialized);

        currentBuffer.AddVertex(ref vertex);

        if (!currentBuffer.IsFull) return;

        currentBuffer.DrawBuffer();
        currentBufferIndex = (currentBufferIndex + 1) % maxBuffers;

        // Next buffer is null
        if (buffers[currentBufferIndex] is null)
        {
            buffers[currentBufferIndex] = currentBuffer = CreateVertexBuffer(size);
            currentBuffer.Initialize();
            return;
        }

        currentBuffer = buffers[currentBufferIndex]!;
        currentBuffer.Reset();
    }

    public void FlushBatch()
    {
        if (currentBuffer.Count > 0)
            currentBuffer.DrawBuffer();
    }

    public void ResetBatch()
    {
        Debug.Assert(buffers is not null);

        currentBuffer = buffers[currentBufferIndex = 0]!;
        currentBuffer.Reset();
    }

    protected abstract VertexBuffer<T> CreateVertexBuffer(int size);

    public void Dispose()
    {
        if (!isInitialized)
            return;

        foreach (VertexBuffer<T>? buffer in buffers)
            buffer?.Dispose();

        GC.SuppressFinalize(this);
    }
}
