using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct PositionAndColourVertex : IVertex
{
    [VertexMember(VertexAttributeType.Float, 2)]
    public readonly Vector2 VertexPosition;

    [VertexMember(VertexAttributeType.Float, 4)]
    public readonly Vector4 VertexColour;

    public PositionAndColourVertex(Vector2 position, Vector4 colour)
    {
        VertexPosition = position;
        VertexColour = colour;
    }
}
