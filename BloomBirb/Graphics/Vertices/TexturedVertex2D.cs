using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TexturedVertex2D : IVertex, IEquatable<TexturedVertex2D>
{
    public static int Size => PositionAndColourVertex.Size + sizeof(float) * 2;

    public static (VertexAttributeType type, int typeSize, int count)[] Layout => PositionAndColourVertex.Layout.Append((VertexAttributeType.Float, sizeof(float), 2)).ToArray();

    public readonly PositionAndColourVertex PositionAndColour;
    public readonly Vector2 TexturePosition;

    public TexturedVertex2D(Vector2 position, Vector4 colour, Vector2 texturePosition)
    {
        PositionAndColour = new PositionAndColourVertex(position, colour);
        TexturePosition = texturePosition;
    }


    public bool Equals(TexturedVertex2D other)
    {
        return PositionAndColour.Equals(other.PositionAndColour) && TexturePosition.Equals(other.TexturePosition);
    }
}
