namespace BloomFramework.ResourceStores;

public interface IResourceStore
{
    public Stream? Get(string file);
}
