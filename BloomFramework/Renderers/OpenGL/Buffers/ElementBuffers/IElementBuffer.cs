namespace BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;

public interface IElementBuffer
{
    abstract static void Bind(OpenGlRenderer renderer, int numberOfVertices);

    abstract static int ToIndicesCount(int verticesCount);
}
