using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct PositionAndColourVertex : IVertex, IEquatable<PositionAndColourVertex>
{
    public static int Size { get; } = sizeof(float) * 6;

    public static ReadOnlyCollection<VertexLayoutEntry> Layout { get; } = new VertexLayoutEntry[]
    {
        new(VertexAttributeType.Float, 2),
        new(VertexAttributeType.Float, 4),
    }.AsReadOnly();

    public readonly Vector2 VertexPosition;
    public readonly Vector4 VertexColour;

    public PositionAndColourVertex(Vector2 position, Vector4 colour)
    {
        VertexPosition = position;
        VertexColour = colour;
    }

    public bool Equals(PositionAndColourVertex other) =>
        VertexPosition.Equals(other.VertexPosition) && VertexColour.Equals(other.VertexColour);
}
