using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;

namespace BloomFramework.Renderers.OpenGL.Buffers;

public class TripleBuffer<TVertex, TElementBuffer> : IVertexBuffer<TVertex>
    where TVertex : unmanaged, IVertex, IEquatable<TVertex>
    where TElementBuffer : IElementBuffer
{
    private VertexBuffer<TVertex, TElementBuffer>[] buffers = new VertexBuffer<TVertex, TElementBuffer>[3];

    private readonly int verticesPerBuffer;

    public TripleBuffer(OpenGlRenderer renderer, int verticesPerBuffer)
    {
        this.verticesPerBuffer = verticesPerBuffer;

        for (int i = 0; i < 3; ++i)
            buffers[i] = new VertexBuffer<TVertex, TElementBuffer>(renderer, verticesPerBuffer);
    }

    public static IVertexBuffer Create(OpenGlRenderer renderer, int vertexCount) => new TripleBuffer<TVertex, TElementBuffer>(renderer, vertexCount);

    private int currentBufferIndex = 0;
    private int count = 0;

    public void AddVertex(ref TVertex vertex)
    {
        buffers[currentBufferIndex].AddVertex(ref vertex);

        // If count == verticesPerBuffer, we switch to the next buffer immediately
        // The current buffer would already submit itself
        if (++count == verticesPerBuffer)
            Reset();
    }

    public void Bind()
    {
        // No op, binding is done by the actual buffers
    }

    public void Draw()
    {
        // Current buffer is unused
        if (count == 0)
            return;

        buffers[currentBufferIndex].Draw();

        Reset();
    }


    public void Reset()
    {
        if (count == 0)
            return;

        currentBufferIndex = (currentBufferIndex + 1) % 3;
        count = 0;
    }

    public bool IsDisposed;
    private void dispose(bool isDisposing)
    {
        if (IsDisposed)
            return;

        if (isDisposing)
        {
            foreach (var buffer in buffers)
                buffer.Dispose();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
    }
}
