using BloomBirb.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL.Buffers;

public class QuadBuffer<T> : VertexBuffer<T>, IDisposable where T : unmanaged, IVertex
{
    protected override PrimitiveType PrimitiveType => PrimitiveType.Triangles;
    protected override int IndicesPerPrimitive => 6;
    protected override int VerticesPerPrimitive => 4;

    private int numberOfQuads;

    public QuadBuffer(int numberOfQuads) : base(numberOfQuads * 4)
    {
        this.numberOfQuads = numberOfQuads;
    }

    protected override uint[] InitializeEBO()
    {
        // 6 indices to render a quad
        uint[] indices = new uint[numberOfQuads * 6];

        for (int i = 0; i < numberOfQuads; ++i)
        {
            indices[i * 6] = (uint)(i * 4);
            indices[(i * 6) + 1] = (uint)(i * 4) + 1;
            indices[(i * 6) + 2] = (uint)(i * 4) + 2;
            indices[(i * 6) + 3] = (uint)(i * 4) + 2;
            indices[(i * 6) + 4] = (uint)(i * 4) + 3;
            indices[(i * 6) + 5] = (uint)(i * 4);
        }

        return indices;
    }
}
