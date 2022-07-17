using Microsoft.Extensions.Caching.Memory;
using Stef.Validation;

namespace RestEase.Authentication.Azure.Extensions;

internal static class MemoryCacheExtensions
{
    public static async Task<TItem> CreateAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
        where TItem : class
    {
        Guard.NotNull(key);

        using var entry = cache.CreateEntry(key);
        var value = (entry.Value = await factory(entry).ConfigureAwait(false));

        return (TItem)value;
    }
}