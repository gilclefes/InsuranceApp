using Microsoft.Extensions.Caching.Memory;

namespace InsuranceApp.Infrastructure.Caching;

public static class ProductCacheVersion
{
    private const string VersionCacheKey = "products:cache-version";
    private static readonly object SyncRoot = new();

    public static int Get(IMemoryCache cache)
    {
        return cache.GetOrCreate(VersionCacheKey, entry =>
        {
            entry.Priority = CacheItemPriority.NeverRemove;
            return 1;
        });
    }

    public static int Bump(IMemoryCache cache)
    {
        lock (SyncRoot)
        {
            var current = Get(cache);
            var next = current == int.MaxValue ? 1 : current + 1;
            cache.Set(VersionCacheKey, next, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });

            return next;
        }
    }
}