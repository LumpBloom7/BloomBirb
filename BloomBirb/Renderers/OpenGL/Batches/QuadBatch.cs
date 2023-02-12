using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL.Buffers;

namespace BloomBirb.Renderers.OpenGL.Batches;

public class QuadBatch<T> : VertexBatch<T> where T : unmanaged, IEquatable<T>, IVertex
{
    protected override VertexBuffer<T> CreateVertexBuffer(int size) => new QuadBuffer<T>(Renderer, size);
}
