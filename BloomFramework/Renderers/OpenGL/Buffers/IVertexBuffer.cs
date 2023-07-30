namespace BloomFramework.Renderers.OpenGL.Buffers;

public interface IVertexBuffer : IDisposable
{
    virtual static IVertexBuffer Create(OpenGlRenderer renderer, int numberOfVertices) => null!;

    void Initialize();

    void Bind();

    /// <summary>
    /// Resets the buffer for next usage
    /// </summary>
    void Reset();

    /// <summary>
    /// Submits the buffer to the GPU for rendering
    /// </summary>
    void Draw();
}

public interface IVertexBuffer<TVertex> : IVertexBuffer
    where TVertex : unmanaged, IEquatable<TVertex>
{
    /// <summary>
    /// Adds a vertex to the buffer
    /// </summary>
    /// <param name="vertex"></param>
    void AddVertex(ref TVertex vertex);
}
