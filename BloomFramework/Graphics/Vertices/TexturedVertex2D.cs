using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomFramework.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TexturedVertex2D : IVertex
{
    public readonly PositionAndColourVertex PositionAndColour;

    [VertexMember(VertexAttributeType.Float, 2)]
    public readonly Vector2 TexturePosition;

    public TexturedVertex2D(Vector2 position, Vector4 colour, Vector2 uvPos)
    {
        PositionAndColour = new PositionAndColourVertex(position, colour);

        TexturePosition = uvPos;
    }
}
