using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL.Buffers;

public abstract unsafe class VertexBuffer<T> : IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    // Ebo sharing
    private static int eboMaxSize;
    private static uint eboHandle;

    protected uint[]? Indices;

    private T* vertices;

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
        vaoHandle = context.GenVertexArray();
        Bind();

        vboHandle = context.GenBuffer();
        context.BindBuffer((GLEnum)BufferTargetARB.ArrayBuffer, vboHandle);
        context.BufferStorage((GLEnum)BufferTargetARB.ArrayBuffer, (nuint)(Size * T.Size), (void**)null, BufferStorageMask.MapReadBit | BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit);

        vertices = (T*)context.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, (nuint)(Size * T.Size), MapBufferAccessMask.ReadBit | MapBufferAccessMask.WriteBit | MapBufferAccessMask.FlushExplicitBit | MapBufferAccessMask.PersistentBit);

        createAndUseEBO();

        GLUtils.SetVAO<T>(context);
    }

    public bool IsFull => Count == Size;

    public int Count { get; private set; }

    private int beginBuffer = -1;
    private int endBuffer = -1;

    public void AddVertex(ref T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        if (!vertices[Count].Equals(vertex))
        {
            vertices[Count] = vertex;

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

    public void BufferData()
    {
        renderer?.Context?.FlushMappedBufferRange(BufferTargetARB.ArrayBuffer, beginBuffer, (nuint)(endBuffer - beginBuffer));
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
            BufferData();

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
