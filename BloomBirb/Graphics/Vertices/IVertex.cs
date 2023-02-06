namespace BloomBirb.Graphics.Vertices;

public interface IVertex
{
    abstract static int Size { get; }

    abstract static (VertexAttributeType type, int count)[] Layout { get; }
}
