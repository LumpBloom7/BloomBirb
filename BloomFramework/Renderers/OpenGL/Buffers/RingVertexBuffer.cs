using System.Runtime.CompilerServices;
using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL;
using BloomFramework.Renderers.OpenGL.Buffers;
using BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;
using Silk.NET.OpenGL;

namespace Namespace;
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
    }

    public static IVertexBuffer Create(OpenGlRenderer renderer, int verticesNumber) => new RingVertexBuffer<TVertex, TElementBuffer>(renderer, verticesNumber);

    private static readonly int vertex_size = Unsafe.SizeOf<TVertex>();

    public unsafe void Initialize()
    {
        vaoHandle = renderer.Context.GenVertexArray();
        vboHandle = renderer.Context.GenBuffer();

        Bind();

        // Allocate the data GPU side
        renderer.Context.BufferStorage(BufferStorageTarget.ArrayBuffer, (nuint)(vertex_size * maxVertices), null,
                                        BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.MapCoherentBit);

        data = (TVertex*)renderer.Context.MapBufferRange(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertex_size * maxVertices), MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit | MapBufferAccessMask.CoherentBit);

        // Set the VAO for this object
        GlUtils.SetVao<TVertex>(renderer.Context);

        // Binds the index buffer to the VAO
        TElementBuffer.Bind(renderer, maxVertices);
    }

    private int startIndex = int.MaxValue;
    private int currentIndex;

    private Queue<FencePoint> fences = new();

    public void AddVertex(ref TVertex vertex)
    {
        if (fences.Count > 0)
        {
            if (fences.Peek().BufferIndex == currentIndex)
            {
                var fencePoint = fences.Dequeue();
                while (true)
                {
                    var result = renderer.Context.ClientWaitSync(fencePoint.FenceHandle, SyncObjectMask.Bit, 1000);
                    if (result is GLEnum.ConditionSatisfied or GLEnum.AlreadySignaled)
                        break;

                    Console.WriteLine("Long running sync point");
                }
                renderer.Context.DeleteSync(fencePoint.FenceHandle);
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
                                            (uint)TElementBuffer.ToIndicesCount(startIndex),
                                            (uint)TElementBuffer.ToIndicesCount(currentIndex),
                                            (uint)TElementBuffer.ToIndicesCount(count),
                                            DrawElementsType.UnsignedInt,
                                            (void*)null);

        Reset();
    }

    public void Reset()
    {
        nint fenceHandle = renderer.Context.FenceSync(SyncCondition.SyncGpuCommandsComplete, SyncBehaviorFlags.None);

        fences.Enqueue(new(startIndex, fenceHandle));

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



    private record struct FencePoint(int BufferIndex, nint FenceHandle);
}
