using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public struct PositionAndColourVertex : IVertex, IEquatable<PositionAndColourVertex>
{
    public static int Size => sizeof(float) * 6;
    public static (VertexAttributeType type, int typeSize, int count)[] Layout => new[]{
        (VertexAttributeType.Float, sizeof(float), 2),
        (VertexAttributeType.Float, sizeof(float), 4),
    };

    public readonly Vector2 VertexPosition;
    public readonly Vector4 VertexColour;

    public PositionAndColourVertex(Vector2 position, Vector4 colour)
    {
        VertexPosition = position;
        VertexColour = colour;
    }

    public bool Equals(PositionAndColourVertex other) => VertexPosition.Equals(other.VertexPosition) && VertexColour.Equals(other.VertexColour);

    public bool Equals(IVertex other) => other is PositionAndColourVertex posColVert && Equals(posColVert);
}