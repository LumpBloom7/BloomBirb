using System.Runtime.CompilerServices;
using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;
using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Buffers;

/// <summary>
/// A single persistent mapped buffer, using fences for synchronization. <br/>
/// In my testing it is a magnitude slower than the regular VertexBuffer, which uses invalidations to speed things up
/// </summary>
/// <typeparam name="TVertex">The vertex type</typeparam>
/// <typeparam name="TElementBuffer">The primitive type</typeparam>
public unsafe class RingVertexBuffer<TVertex, TElementBuffer> : IVertexBuffer<TVertex>
    where TVertex : unmanaged, IVertex, IEquatable<TVertex>
    where TElementBuffer : IElementBuffer
{
    private OpenGlRenderer renderer;

    private int maxVertices;

    private uint vaoHandle;
    private uint vboHandle;

    private TVertex* data;

    public RingVertexBuffer(OpenGlRenderer renderer, int verticesNumber)
    {
        this.renderer = renderer;
        maxVertices = verticesNumber;

        vaoHandle = renderer.Context.GenVertexArray();
        vboHandle = renderer.Context.GenBuffer();

        Bind();

        // Allocate the data GPU side
        renderer.Context.BufferStorage(BufferStorageTarget.ArrayBuffer, (nuint)(vertex_size * maxVertices), null,
                                        BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.MapCoherentBit);

        data = (TVertex*)renderer.Context.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertex_size * maxVertices),
                                                            MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit | MapBufferAccessMask.CoherentBit);

        // Set the VAO for this object
        GlUtils.SetVao<TVertex>(renderer.Context);

        // Binds the index buffer to the VAO
        TElementBuffer.Bind(renderer, maxVertices);vaoHandle = renderer.Context.GenVertexArray();
        vboHandle = renderer.Context.GenBuffer();

        Bind();

        // Allocate the data GPU side
        renderer.Context.BufferStorage(BufferStorageTarget.ArrayBuffer, (nuint)(vertex_size * maxVertices), null,
                                        BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.MapCoherentBit);

        data = (TVertex*)renderer.Context.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertex_size * maxVertices),
                                                            MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit | MapBufferAccessMask.CoherentBit);

        // Set the VAO for this object
        GlUtils.SetVao<TVertex>(renderer.Context);

        // Binds the index buffer to the VAO
        TElementBuffer.Bind(renderer, maxVertices);
    }

    public static IVertexBuffer Create(OpenGlRenderer renderer, int verticesNumber) => new RingVertexBuffer<TVertex, TElementBuffer>(renderer, verticesNumber);

    private static readonly int vertex_size = Unsafe.SizeOf<TVertex>();

    private int startIndex;
    private int currentIndex;

    private Queue<FencePoint> fences = new();

    public void AddVertex(ref TVertex vertex)
    {
        if (fences.Count > 0)
        {
            if (fences.Peek().BufferIndex == currentIndex)
            {
                var fencePoint = fences.Dequeue();
                while (!fencePoint.Fence.IsSignalled)
                {
                    Console.WriteLine($"Long running sync point @ : {currentIndex}");
                }
            }
        }

        data[currentIndex] = vertex;

        ++currentIndex;

        if (currentIndex == maxVertices)
            Draw();
    }

    public unsafe void Draw()
    {
        if (currentIndex == startIndex)
            return;

        renderer.BindVertexArray(vaoHandle);

        int count = currentIndex - startIndex;

        renderer.Context.DrawRangeElements(PrimitiveType.Triangles,
                                            (uint)startIndex,
                                            (uint)currentIndex - 1,
                                            (uint)TElementBuffer.ToIndicesCount(count),
                                            DrawElementsType.UnsignedInt,
                                            (void*)(TElementBuffer.ToIndicesCount(startIndex) * sizeof(uint)));

        Reset();
    }

    public void Reset()
    {
        GLFence fence = renderer.CreateFence();

        fences.Enqueue(new(startIndex, fence));

        if (currentIndex == maxVertices)
            currentIndex = 0;

        startIndex = currentIndex;
    }

    public void Bind()
    {
        renderer.BindVertexArray(vaoHandle);

        // This needs to be bound if we want to upload data
        // But doesn't need to be bound if we are simply drawing it
        renderer.BindBuffer(vboHandle);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private record struct FencePoint(int BufferIndex, GLFence Fence);
}
