using System.Runtime.CompilerServices;
using BloomFramework.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Buffers;

public abstract unsafe class VertexBuffer<T> : IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    private readonly T[] vertices;

    private uint vboHandle;

    private uint vaoHandle;

    private readonly int size;

    protected abstract PrimitiveType PrimitiveType { get; }
    protected abstract int IndicesPerPrimitive { get; }
    protected abstract int VerticesPerPrimitive { get; }

    protected readonly OpenGlRenderer Renderer;

    private static readonly int vertex_size = Unsafe.SizeOf<T>();

    protected VertexBuffer(OpenGlRenderer renderer, int amountOfVertices = 10000)
    {
        Renderer = renderer;
        size = amountOfVertices;
        vertices = new T[amountOfVertices];
    }

    public void Initialize()
    {
        vaoHandle = Renderer.Context.GenVertexArray();
        vboHandle = Renderer.Context.GenBuffer();

        bind();

        Renderer.Context.BufferData((GLEnum)BufferTargetARB.ArrayBuffer, (nuint)(size * vertex_size), null, BufferUsageARB.DynamicDraw);

        InitializeEbo();

        GlUtils.SetVao<T>(Renderer.Context);
    }

    public bool IsFull => (batchBegin + Count) == size;

    public int Count { get; private set; }

    private int batchBegin;

    private int changeBegin = int.MaxValue;
    private int changeEnd = int.MinValue;

    public void AddVertex(ref T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        int index = batchBegin + Count;

        if (!vertices[index].Equals(vertex))
        {
            vertices[index] = vertex;

            changeBegin = Math.Min(changeBegin, index);
            changeEnd = Math.Max(changeEnd, index + 1);
        }

        ++Count;
    }

    public void Reset()
    {
        Count = 0;
        batchBegin = 0;
    }

    private void bufferData()
    {
        Renderer.Context.BufferSubData(BufferTargetARB.ArrayBuffer, changeBegin * vertex_size, new ReadOnlySpan<T>(vertices, changeBegin, changeEnd - changeBegin));

        changeBegin = int.MaxValue;
        changeEnd = int.MinValue;
    }

    private void bind()
    {
        Renderer.BindVertexArray(vaoHandle);
        Renderer.BindBuffer(vboHandle);
    }

    protected abstract void InitializeEbo();

    public void DrawBuffer()
    {
        if (Count == 0)
            return;

        bind();

        if (changeBegin < int.MaxValue)
            bufferData();

        int indicesToDraw = Count / VerticesPerPrimitive * IndicesPerPrimitive;
        int indicesOffset = batchBegin / VerticesPerPrimitive * IndicesPerPrimitive * sizeof(uint);
        Renderer.Context.DrawElements(PrimitiveType, (uint)indicesToDraw, DrawElementsType.UnsignedInt, (void*)indicesOffset);

        batchBegin += Count;
        Count = 0;
    }

    private bool isDisposed;

    protected void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        Renderer.Context.DeleteVertexArray(vaoHandle);
        Renderer.Context.DeleteBuffer(vboHandle);
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
