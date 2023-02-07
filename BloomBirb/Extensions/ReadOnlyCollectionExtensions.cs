using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace BloomBirb.Extensions;

public static class ReadonlyCollectionExtensions
{
    public static ReadOnlyCollection<T> AddRange<T>(this ReadOnlyCollection<T> collection, params T[] values)
    {
        var builder = new ReadOnlyCollectionBuilder<T>(collection);
        foreach (var value in values)
            builder.Add(value);

        return builder.ToReadOnlyCollection();
    }
}
