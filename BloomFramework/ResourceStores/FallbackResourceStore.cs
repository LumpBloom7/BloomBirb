namespace BloomFramework.ResourceStores;

public class FallbackResourceStore : IResourceStore
{
    private readonly IResourceStore primaryStore;
    private readonly IResourceStore fallbackStore;

    public FallbackResourceStore(IResourceStore primary, IResourceStore fallback)
    {
        primaryStore = primary;
        fallbackStore = fallback;
    }

    public FallbackResourceStore(IResourceStore primary)
    {
        primaryStore = primary;
        fallbackStore = new EmbeddedResourceStore();
    }

    public Stream? Get(string file) => primaryStore.Get(file) ?? fallbackStore.Get(file);
}
