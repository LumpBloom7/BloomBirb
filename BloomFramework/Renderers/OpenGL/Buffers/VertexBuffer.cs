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

        vboHandle = renderer.Context.GenBuffer();
        vaoHandle = renderer.Context.GenVertexArray();

        Bind();

        unsafe
        {
            // Allocate the data GPU side
            renderer.Context.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertex_size * data.Length), null, BufferUsageARB.DynamicDraw);
        }

        // Set the VAO for this object
        GlUtils.SetVao<TVertex>(renderer.Context);

        // Binds the index buffer to the VAO
        TElementBuffer.Bind(renderer, data.Length);
    }

    public static IVertexBuffer Create(OpenGlRenderer renderer, int vertexCount) => new VertexBuffer<TVertex, TElementBuffer>(renderer, vertexCount);

    public void Bind()
    {
        renderer.BindVertexArray(vaoHandle);

        // This needs to be bound if we want to upload data
        // But doesn't need to be bound if we are simply drawing it
        renderer.BindBuffer(vboHandle);
    }

    private int currentIndex;

    public void AddVertex(ref TVertex vertex)
    {
        data[currentIndex] = vertex;
        ++currentIndex;

        // The buffer is full, force draw immediately
        if (currentIndex == data.Length)
            Draw();
    }

    public unsafe void Draw()
    {
        // Nothing to draw
        if (currentIndex == 0)
            return;

        Bind();

        // This allows the underlying buffer to be swapped out, without an implicit sync
        renderer.Context.InvalidateBufferData(vboHandle);
        renderer.Context.BufferSubData(BufferTargetARB.ArrayBuffer, 0, new ReadOnlySpan<TVertex>(data, 0, currentIndex));

        unsafe
        {
            renderer.Context.DrawElements(PrimitiveType.Triangles, (uint)TElementBuffer.ToIndicesCount(currentIndex), DrawElementsType.UnsignedInt, (void*)null);
        }

        // Prepare for next use
        Reset();
    }

    public void Reset()
    {
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
