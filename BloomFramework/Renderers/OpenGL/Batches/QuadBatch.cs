using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL.Buffers;

namespace BloomFramework.Renderers.OpenGL.Batches;

public class QuadBatch<T> : VertexBatch<T> where T : unmanaged, IEquatable<T>, IVertex
{
    protected override VertexBuffer<T> CreateVertexBuffer(int size) => new QuadBuffer<T>(Renderer, size);
}
