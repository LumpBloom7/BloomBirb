using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL.Buffers;

public abstract class VertexBuffer<T> : IDisposable where T : unmanaged, IVertex
{
    protected uint[]? Indices;
    protected T[]? Vertices;

    private uint vboHandle;
    private uint eboHandle;
    private uint vaoHandle;

    protected readonly int Size;

    protected abstract PrimitiveType PrimitiveType { get; }

    private GL? gl;

    public VertexBuffer(int amountOfVertices)
    {
        Size = amountOfVertices;
    }

    public void Initialize(GL gl)
    {
        this.gl = gl;

        Vertices = new T[Size];
        Indices = InitializeEBO();

        vaoHandle = gl.GenVertexArray();
        Bind();

        vboHandle = gl.GenBuffer();
        gl.BindBuffer((GLEnum)BufferTargetARB.ArrayBuffer, vboHandle);
        gl.BufferData((GLEnum)BufferTargetARB.ArrayBuffer, (nuint)Size, (nuint)null, BufferUsageARB.StaticDraw);

        eboHandle = gl.GenBuffer();
        gl.BindBuffer((GLEnum)BufferTargetARB.ElementArrayBuffer, eboHandle);
        gl.BufferData((GLEnum)BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(Indices), BufferUsageARB.StaticDraw);

        GLUtils.SetVAO<TexturedVertex2D>(gl);
    }

    public void BufferData(ReadOnlySpan<T> data, int offset)
    {
        if (offset + data.Length > Size)
            throw new IndexOutOfRangeException("An attempt to store data outside of buffer bounds");

        gl?.BindBuffer((GLEnum)BufferTargetARB.ArrayBuffer, vboHandle);

        // If we are replacing the entire buffer, we just ask for a new buffer from GL so we don't have to sync up with the driver
        if (data.Length + offset == Size)
            gl?.BufferData(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.StaticDraw);
        else
            gl?.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset * T.Size, data);
    }

    public void Bind() => gl?.BindVertexArray(vaoHandle);

    protected abstract uint[] InitializeEBO();

    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (gl is null)
            return;

        gl.DeleteVertexArray(vaoHandle);
        gl.DeleteBuffer(vboHandle);
        gl.DeleteBuffer(eboHandle);
        isDisposed = true;
    }

    public void DrawBuffer() => DrawBuffer((uint)(Indices?.Length));
    public unsafe void DrawBuffer(uint count)
    {
        Bind();
        gl?.DrawElements(PrimitiveType, count, DrawElementsType.UnsignedInt, (void*)null);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~VertexBuffer()
    {
        Dispose(false);
    }
}
