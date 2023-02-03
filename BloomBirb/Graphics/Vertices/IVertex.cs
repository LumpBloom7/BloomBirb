namespace BloomBirb.Graphics.Vertices;

public interface IVertex
{
    abstract static int Size { get; }

    abstract static (VertexAttributeType type, int typeSize, int count)[] Layout { get; }

    bool Equals(IVertex other);
}
