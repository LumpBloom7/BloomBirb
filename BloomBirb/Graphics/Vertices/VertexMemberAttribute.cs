using System.Diagnostics.CodeAnalysis;

namespace BloomBirb.Graphics.Vertices;

[AttributeUsage(AttributeTargets.Field)]
public sealed class VertexMemberAttribute : Attribute
{
    public required VertexAttributeType AttributeType { get; init; }
    public required int Count { get; set; } = 1;

    [SetsRequiredMembers]
    public VertexMemberAttribute(VertexAttributeType attributeType, int count = 1)
    {
        AttributeType = attributeType;
        Count = count;
    }
}
