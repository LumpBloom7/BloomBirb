using BloomBirb.Graphics.Vertices;

namespace BloomBirb.Renderers.OpenGL.Batches;

public interface IVertexBatch<T> : IVertexBatch where T : unmanaged, IEquatable<T>, IVertex
{
    void AddVertex(T vertex);
}

public interface IVertexBatch
{
    void Initialize(OpenGLRenderer renderer, int bufferSize, int maxNumberOfBuffers);
    void FlushBatch();
}
