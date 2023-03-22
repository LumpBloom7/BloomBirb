using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;
using BloomBirb.Extensions;
using BloomBirb.Renderers.OpenGL.Textures;

namespace BloomBirb.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TexturedVertex2D : IVertex, IEquatable<TexturedVertex2D>
{
    public static int Size => PositionAndColourVertex.Size + sizeof(float) * 6;

    public static ReadOnlyCollection<VertexLayoutEntry> Layout { get; } = PositionAndColourVertex.Layout.AddRange(
        new VertexLayoutEntry(VertexAttributeType.Float, 2),
        new VertexLayoutEntry(VertexAttributeType.Float, 2),
        new VertexLayoutEntry(VertexAttributeType.Float, 2)
    );

    public readonly PositionAndColourVertex PositionAndColour;
    public readonly Vector2 TexturePosition;
    public readonly Vector2 TextureRegionOrigin;
    public readonly Vector2 TextureRegionSize;

    public TexturedVertex2D(Vector2 position, Vector4 colour, Vector2 texturePosition, TextureUsage texture)
    {
        PositionAndColour = new PositionAndColourVertex(position, colour);
        TexturePosition = texturePosition;
        TextureRegionOrigin = new(texture.TextureRegion.Origin.X, texture.TextureRegion.Origin.Y);
        TextureRegionSize = new(texture.TextureRegion.Size.X, texture.TextureRegion.Size.Y);
    }
}
