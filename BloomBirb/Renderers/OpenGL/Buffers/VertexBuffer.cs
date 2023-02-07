using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL.Buffers;

public abstract class VertexBuffer<T> : IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    // Ebo sharing
    private static int eboMaxSize;
    private static uint eboHandle;

    protected uint[]? Indices;
    protected T[]? Vertices;

    private uint vboHandle;

    private uint vaoHandle;

    protected readonly int Size;

    protected abstract PrimitiveType PrimitiveType { get; }
    protected abstract int IndicesPerPrimitive { get; }
    protected abstract int VerticesPerPrimitive { get; }

    private readonly OpenGLRenderer renderer;
    private GL context => renderer.Context!;

    public VertexBuffer(OpenGLRenderer renderer, int amountOfVertices = 10000)
    {
        this.renderer = renderer;
        Size = amountOfVertices;
    }

    private void createAndUseEBO()
    {
        int desiredSize = Size / VerticesPerPrimitive * IndicesPerPrimitive;

        if (eboHandle == 0)
            eboHandle = context.GenBuffer();

        context.BindBuffer((GLEnum)BufferTargetARB.ElementArrayBuffer, eboHandle);

        if (eboMaxSize >= desiredSize)
            return;

        eboMaxSize = desiredSize;
        Indices = InitializeEBO();

        context.BufferData((GLEnum)BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(Indices),
            BufferUsageARB.DynamicDraw);
    }

    public unsafe void Initialize()
    {
        Vertices = new T[Size];

        vaoHandle = context.GenVertexArray();
        Bind();

        vboHandle = context.GenBuffer();
        context.BindBuffer((GLEnum)BufferTargetARB.ArrayBuffer, vboHandle);
        context.BufferData((GLEnum)BufferTargetARB.ArrayBuffer, (nuint)(Size * T.Size), (void**)null,
            BufferUsageARB.DynamicDraw);

        createAndUseEBO();

        GLUtils.SetVAO<T>(context);
    }

    public bool IsFull => Count == Size;

    public int Count { get; private set; }

    private int beginBuffer = -1;
    private int endBuffer = -1;

    public void AddVertex(ref T vertex)
    {
        ArgumentNullException.ThrowIfNull(Vertices);

        if (!Vertices[Count].Equals(vertex))
        {
            Vertices[Count] = vertex;

            if (beginBuffer == -1)
            {
                beginBuffer = Count;
                endBuffer = Count + 1;
            }
            else
            {
                endBuffer = Count + 1;
            }
        }

        ++Count;
    }

    private void reset()
    {
        Count = 0;
        beginBuffer = -1;
        endBuffer = -1;
    }

    public void BufferData(ReadOnlySpan<T> data, int offset)
    {
        if (offset + data.Length > Size)
            throw new IndexOutOfRangeException("An attempt to store data outside of buffer bounds");

        // If we are replacing the entire buffer, we just ask for a new buffer from GL so we don't have to sync up with the driver
        if (data.Length == Size || offset == 0)
            context?.BufferData(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.DynamicDraw);
        else
            context?.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset * T.Size, data);
    }

    public void Bind()
    {
        context?.BindVertexArray(vaoHandle);
        context?.BindBuffer(BufferTargetARB.ArrayBuffer, vboHandle);
    }

    protected abstract uint[] InitializeEBO();

    public unsafe void DrawBuffer()
    {
        Bind();

        if (beginBuffer != -1)
            BufferData(new ReadOnlySpan<T>(Vertices, beginBuffer, endBuffer - beginBuffer), beginBuffer);

        int indicesToDraw = Count / VerticesPerPrimitive * IndicesPerPrimitive;
        context?.DrawElements(PrimitiveType, (uint)indicesToDraw, DrawElementsType.UnsignedInt, (void*)null);

        reset();
    }

    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        context.DeleteVertexArray(vaoHandle);
        context.DeleteBuffer(vboHandle);
        context.DeleteBuffer(eboHandle);
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
