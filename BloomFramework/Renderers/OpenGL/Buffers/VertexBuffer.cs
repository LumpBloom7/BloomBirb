using Silk.NET.OpenGL;
using BloomFramework.Graphics.Vertices;
using System.Runtime.CompilerServices;
using BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;

namespace BloomFramework.Renderers.OpenGL.Buffers;

public class VertexBuffer<TVertex, TElementBuffer> : IVertexBuffer<TVertex>
    where TVertex : unmanaged, IVertex, IEquatable<TVertex>
    where TElementBuffer : IElementBuffer
{
    private readonly TVertex[] data;
    private readonly OpenGlRenderer renderer;

    private uint vboHandle;
    private uint vaoHandle;

    private static readonly int vertex_size = Unsafe.SizeOf<TVertex>();

    public VertexBuffer(OpenGlRenderer renderer, int vertexCount)
    {
        data = new TVertex[vertexCount];
        this.renderer = renderer;
    }

    public static IVertexBuffer Create(OpenGlRenderer renderer, int vertexCount) => new VertexBuffer<TVertex, TElementBuffer>(renderer, vertexCount);

    // I'd rather follow RAII here, this should be done in the constructor
    public unsafe void Initialize()
    {
        vboHandle = renderer.Context.GenBuffer();
        vaoHandle = renderer.Context.GenVertexArray();

        Bind();

        // Allocate the data GPU side
        renderer.Context.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertex_size * data.Length), null, BufferUsageARB.DynamicDraw);

        // Set the VAO for this object
        GlUtils.SetVao<TVertex>(renderer.Context);

        // Binds the index buffer to the VAO
        TElementBuffer.Bind(renderer, data.Length);
    }

    public void Bind()
    {
        renderer.BindVertexArray(vaoHandle);

        // This needs to be bound if we want to upload data
        // But doesn't need to be bound if we are simply drawing it
        renderer.BindBuffer(vboHandle);
    }

    private int beginIndex = int.MaxValue;
    private int endIndex = int.MinValue;
    private int currentIndex;

    public void AddVertex(ref TVertex vertex)
    {
        // We don't want to resubmit equivalent data
        if (!data[currentIndex].Equals(vertex))
        {
            data[currentIndex] = vertex;

            beginIndex = Math.Min(beginIndex, currentIndex);
            endIndex = Math.Max(endIndex, currentIndex + 1);
        }
        ++currentIndex;

        // The buffer is full, force draw immediately
        if (currentIndex == data.Length)
            Draw();
    }

    public void Draw()
    {
        // Nothing to draw
        if (currentIndex == 0)
            return;

        // This is wasteful, since we need not bind the buffer manually if we don't need to upload data
        Bind();

        // Upload updated data
        if (beginIndex < endIndex)
        {
            int changedVerticesCount = endIndex - beginIndex;
            renderer.Context.BufferSubData(BufferTargetARB.ArrayBuffer, vertex_size * beginIndex, new ReadOnlySpan<TVertex>(data, beginIndex, changedVerticesCount));
        }

        unsafe
        {
            renderer.Context.DrawElements(PrimitiveType.Triangles, (uint)TElementBuffer.ToIndicesCount(currentIndex), DrawElementsType.UnsignedInt, (void*)null);
        }

        // Prepare for next use
        Reset();
    }

    public void Reset()
    {
        beginIndex = int.MaxValue;
        endIndex = int.MinValue;
        currentIndex = 0;
    }


    public bool IsDisposed { get; private set; }

    private void dispose(bool isDisposing)
    {
        if (IsDisposed)
            return;

        renderer.Context.DeleteVertexArray(vaoHandle);
        renderer.Context.DeleteBuffer(vboHandle);

        IsDisposed = true;
    }

    public void Dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
    }

    ~VertexBuffer()
    {
        dispose(false);
    }
}
