using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using BloomBirb.Extensions;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct DepthWrappingVertex<T> : IVertex, IEquatable<DepthWrappingVertex<T>> where T : unmanaged, IEquatable<T>, IVertex
{
    public static int Size { get; } = T.Size + sizeof(float);

    public static ReadOnlyCollection<VertexLayoutEntry> Layout { get; } = T.Layout.AddRange(
        new VertexLayoutEntry(VertexAttributeType.Float, 1)
    );

    public readonly T Vertex;

    public readonly float Depth;

    public DepthWrappingVertex(T vertex, float depth)
    {
        Vertex = vertex;
        Depth = Math.Min(1f, depth);
    }

    public bool Equals(DepthWrappingVertex<T> other) => Vertex.Equals(other.Vertex) && Depth == other.Depth;
}
