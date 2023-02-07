namespace BloomBirb.ResourceStores;

public interface IResourceStore
{
    public Stream? Get(string file);
}
