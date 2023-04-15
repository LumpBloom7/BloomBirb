using System.Runtime.CompilerServices;
using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL.Buffers;

public abstract unsafe class VertexBuffer<T> : IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    protected uint[]? Indices;

    private T[] vertices;

    private uint vboHandle;

    private uint vaoHandle;

    protected readonly int Size;

    protected abstract PrimitiveType PrimitiveType { get; }
    protected abstract int IndicesPerPrimitive { get; }
    protected abstract int VerticesPerPrimitive { get; }

    protected readonly OpenGLRenderer Renderer;
    private GL context => Renderer.Context!;

    private static int vertexSize = Unsafe.SizeOf<T>();

    public VertexBuffer(OpenGLRenderer renderer, int amountOfVertices = 10000)
    {
        Renderer = renderer;
        Size = amountOfVertices;
        vertices = new T[amountOfVertices];
    }

    public unsafe void Initialize()
    {
        vaoHandle = context.GenVertexArray();
        vboHandle = context.GenBuffer();

        Bind();

        context.BufferData((GLEnum)BufferTargetARB.ArrayBuffer, (nuint)(Size * vertexSize), (void**)null, BufferUsageARB.DynamicDraw);

        InitializeEBO();

        GLUtils.SetVAO<T>(context);
    }

    public bool IsFull => (batchBegin + Count) == Size;

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

    public void BufferData()
    {
        Renderer?.Context?.BufferSubData(BufferTargetARB.ArrayBuffer, changeBegin * vertexSize, new ReadOnlySpan<T>(vertices, changeBegin, changeEnd - changeBegin));

        changeBegin = int.MaxValue;
        changeEnd = int.MinValue;
    }

    public void Bind()
    {
        Renderer.BindVertexArray(vaoHandle);
        Renderer.BindBuffer(vboHandle);
    }

    protected abstract void InitializeEBO();


    public unsafe void DrawBuffer()
    {
        if (Count == 0)
            return;

        Bind();

        if (changeBegin < int.MaxValue)
            BufferData();

        int indicesToDraw = Count / VerticesPerPrimitive * IndicesPerPrimitive;
        int indicesOffset = batchBegin / VerticesPerPrimitive * IndicesPerPrimitive * sizeof(uint);
        context?.DrawElements(PrimitiveType, (uint)indicesToDraw, DrawElementsType.UnsignedInt, (void*)indicesOffset);

        batchBegin += Count;
        Count = 0;
    }

    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        context.DeleteVertexArray(vaoHandle);
        context.DeleteBuffer(vboHandle);
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
