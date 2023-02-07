using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;
using BloomBirb.Extensions;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TexturedVertex2D : IVertex, IEquatable<TexturedVertex2D>
{
    public static int Size => PositionAndColourVertex.Size + sizeof(float) * 2;

    public static ReadOnlyCollection<VertexLayoutEntry> Layout { get; } = PositionAndColourVertex.Layout.AddRange(
        new VertexLayoutEntry(VertexAttributeType.Float, 2)
    );

    public readonly PositionAndColourVertex PositionAndColour;
    public readonly Vector2 TexturePosition;

    public TexturedVertex2D(Vector2 position, Vector4 colour, Vector2 texturePosition)
    {
        PositionAndColour = new PositionAndColourVertex(position, colour);
        TexturePosition = texturePosition;
    }

    public bool Equals(TexturedVertex2D other) => PositionAndColour.Equals(other.PositionAndColour) && TexturePosition.Equals(other.TexturePosition);
}
