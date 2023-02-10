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

    private static float nextDepth;
    public static void Reset() => nextDepth = -1;
    public static void Increment() => nextDepth += 0.001f;

    public DepthWrappingVertex(T vertex)
    {
        Vertex = vertex;
        Depth = nextDepth;
    }

    public static implicit operator DepthWrappingVertex<T>(T vertex) => new(vertex);

    public bool Equals(DepthWrappingVertex<T> other) => Vertex.Equals(other.Vertex) && Depth == other.Depth;


}
