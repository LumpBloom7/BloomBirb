using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL.Buffers;

internal static class QuadIndexBuffer
{
    public static void InitializeEBO(OpenGLRenderer renderer, int desiredIndices)
    {
        if (eboHandle == 0)
            eboHandle = renderer.Context!.GenBuffer();

        renderer.Context!.BindBuffer(BufferTargetARB.ElementArrayBuffer, eboHandle);

        if (maxIndices < desiredIndices)
        {
            uint[] buffer = new uint[desiredIndices];
            for (int i = 0, j = 0; i < desiredIndices; i += 6, j += 4)
            {
                buffer[i] = (uint)j;
                buffer[i + 1] = (uint)j + 1;
                buffer[i + 2] = (uint)j + 2;
                buffer[i + 3] = (uint)j + 2;
                buffer[i + 4] = (uint)j + 3;
                buffer[i + 5] = (uint)j;
            }

            renderer.Context!.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(buffer), BufferUsageARB.DynamicDraw);
            maxIndices = desiredIndices;
        }
    }

    private static uint eboHandle;
    private static int maxIndices;
}

public class QuadBuffer<T> : VertexBuffer<T>, IDisposable where T : unmanaged, IEquatable<T>, IVertex
{
    protected override PrimitiveType PrimitiveType => PrimitiveType.Triangles;
    protected override int IndicesPerPrimitive => 6;
    protected override int VerticesPerPrimitive => 4;

    private int numberOfQuads;

    public QuadBuffer(OpenGLRenderer renderer, int numberOfQuads) : base(renderer, numberOfQuads * 4)
    {
        this.numberOfQuads = numberOfQuads;
    }

    protected override void InitializeEBO() => QuadIndexBuffer.InitializeEBO(Renderer, numberOfQuads * 6);
}
