namespace BloomBirb.Graphics.Vertices;

[AttributeUsage(AttributeTargets.Field)]
public sealed class VertexMemberAttribute : Attribute
{
    public readonly VertexAttributeType AttributeType;
    public readonly int Count;

    public VertexMemberAttribute(VertexAttributeType attributeType, int count = 1)
    {
        AttributeType = attributeType;
        Count = count;
    }
}
