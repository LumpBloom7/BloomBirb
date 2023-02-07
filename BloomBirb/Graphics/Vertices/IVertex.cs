using System.Collections.ObjectModel;

namespace BloomBirb.Graphics.Vertices;

public interface IVertex
{
    abstract static int Size { get; }

    abstract static ReadOnlyCollection<VertexLayoutEntry> Layout { get; }
}
