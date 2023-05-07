using System.Numerics;
using System.Runtime.InteropServices;
using BloomFramework.Renderers.OpenGL.Textures;

namespace BloomFramework.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TexturedVertex2D : IVertex
{
    public readonly PositionAndColourVertex PositionAndColour;

    [VertexMember(VertexAttributeType.Float, 2)]
    public readonly Vector2 TexturePosition;

    [VertexMember(VertexAttributeType.Float, 2)]
    public readonly Vector2 TextureRegionOrigin;


    [VertexMember(VertexAttributeType.Float, 2)]
    public readonly Vector2 TextureRegionSize;

    public TexturedVertex2D(Vector2 position, Vector4 colour, Vector2 texturePosition, TextureUsage texture)
    {
        PositionAndColour = new PositionAndColourVertex(position, colour);
        TexturePosition = texturePosition;
        TextureRegionOrigin = new(texture.TextureRegion.Origin.X, texture.TextureRegion.Origin.Y);
        TextureRegionSize = new(texture.TextureRegion.Size.X, texture.TextureRegion.Size.Y);
    }
}
