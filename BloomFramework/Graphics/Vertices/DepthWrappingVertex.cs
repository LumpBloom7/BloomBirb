using System.Runtime.InteropServices;

namespace BloomFramework.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct DepthWrappingVertex<T> : IVertex where T : unmanaged, IEquatable<T>, IVertex
{
    public readonly T Vertex;

    [VertexMember(VertexAttributeType.Float)]
    public readonly float Depth;

    public DepthWrappingVertex(T vertex, float depth)
    {
        Vertex = vertex;
        Depth = Math.Min(1f, depth);
    }
}
