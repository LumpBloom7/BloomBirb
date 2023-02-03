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
    protected abstract int IndicesPerPrimitive { get; }
    protected abstract int VerticesPerPrimitive { get; }

    private GL? gl;

    public VertexBuffer(int amountOfVertices = 10000)
    {
        Size = amountOfVertices;
    }

    public unsafe void Initialize(GL gl)
    {
        this.gl = gl;

        Vertices = new T[Size];
        Indices = InitializeEBO();

        vaoHandle = gl.GenVertexArray();
        Bind();

        vboHandle = gl.GenBuffer();
        gl.BindBuffer((GLEnum)BufferTargetARB.ArrayBuffer, vboHandle);
        gl.BufferData((GLEnum)BufferTargetARB.ArrayBuffer, (nuint)(Size * T.Size), (void**)null, BufferUsageARB.DynamicDraw);

        eboHandle = gl.GenBuffer();
        gl.BindBuffer((GLEnum)BufferTargetARB.ElementArrayBuffer, eboHandle);
        gl.BufferData((GLEnum)BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(Indices), BufferUsageARB.StaticDraw);

        GLUtils.SetVAO<TexturedVertex2D>(gl);
    }

    private int count;
    private int beginBuffer = -1;
    private int endBuffer = -1;

    public void AddVertex(T vertex)
    {
        ArgumentNullException.ThrowIfNull(Vertices);

        if (!Vertices[count].Equals(vertex))
        {
            Vertices[count] = vertex;

            if (beginBuffer == -1)
            {
                beginBuffer = count;
                endBuffer = count + 1;
            }
            else
            {
                endBuffer = count + 1;
            }
        }

        ++count;
    }

    private void reset()
    {
        count = 0;
        beginBuffer = -1;
        endBuffer = -1;
    }

    public void BufferData(ReadOnlySpan<T> data, int offset)
    {
        if (offset + data.Length > Size)
            throw new IndexOutOfRangeException("An attempt to store data outside of buffer bounds");

        gl?.BindBuffer((GLEnum)BufferTargetARB.ArrayBuffer, vboHandle);

        // If we are replacing the entire buffer, we just ask for a new buffer from GL so we don't have to sync up with the driver
        if (data.Length == Size)
            gl?.BufferData(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.DynamicDraw);
        else
            gl?.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset * T.Size, data);
    }

    public void Bind() => gl?.BindVertexArray(vaoHandle);

    protected abstract uint[] InitializeEBO();

    public unsafe void DrawBuffer()
    {
        if (beginBuffer != -1)
            BufferData(new ReadOnlySpan<T>(Vertices, beginBuffer, endBuffer - beginBuffer), beginBuffer);

        Bind();

        int indicesToDraw = count / VerticesPerPrimitive * IndicesPerPrimitive;
        gl?.DrawElements(PrimitiveType, (uint)indicesToDraw, DrawElementsType.UnsignedInt, (void*)null);

        reset();
    }

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
