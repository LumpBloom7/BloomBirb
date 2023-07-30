using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL.Buffers.ElementBuffers;

public class QuadElementBuffer : IElementBuffer
{
    private static uint eboHandle;

    private static int maxVertices;

    private const float vertices_to_indices_factor = 6f / 4f;

    public static void Bind(OpenGlRenderer renderer, int numberOfVertices)
    {
        if (eboHandle == 0)
            eboHandle = renderer.Context.GenBuffer();

        renderer.Context.BindBuffer(BufferTargetARB.ElementArrayBuffer, eboHandle);

        int numberOfIndices = ToIndicesCount(numberOfVertices); // 4 vertices per quad, with 6 indices per quad

        if (maxVertices >= numberOfIndices)
            return;

        maxVertices = numberOfVertices;

        uint[] indices = new uint[numberOfIndices];

        // i is the current index, j is the current vertex
        for (int i = 0, j = 0; i < numberOfIndices; i += 6, j += 4)
        {
            indices[i] = (uint)j;
            indices[i + 1] = (uint)j + 1;
            indices[i + 2] = (uint)j + 2;
            indices[i + 3] = (uint)j + 2;
            indices[i + 4] = (uint)j + 3;
            indices[i + 5] = (uint)j;
        }

        renderer.Context.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(indices), BufferUsageARB.StaticDraw);
    }

    public static int ToIndicesCount(int verticesCount) => (int)(verticesCount * vertices_to_indices_factor);
}
