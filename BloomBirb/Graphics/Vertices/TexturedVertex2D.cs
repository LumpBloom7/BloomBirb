using System.Numerics;
using System.Runtime.InteropServices;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct TexturedVertex2D : IVertex
{
    public static int Size => sizeof(float) * 4;

    public static (VertexAttributeType type, int typeSize, int count)[] Layout => new[]{
        (VertexAttributeType.Float, sizeof(float), 2),
        (VertexAttributeType.Float, sizeof(float), 2),
    };

    public readonly Vector2 VertexPosition;
    public readonly Vector2 TexturePosition;

    public TexturedVertex2D(Vector2 vertexPosition, Vector2 texturePosition)
    {
        VertexPosition = vertexPosition;
        TexturePosition = texturePosition;
    }

    public TexturedVertex2D(float x, float y, float u, float v) : this(new(x, y), new(u, v)) { }
}
