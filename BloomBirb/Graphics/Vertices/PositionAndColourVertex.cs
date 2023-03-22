using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct PositionAndColourVertex : IVertex
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
}
