using Microsoft.Extensions.Caching.Memory;

namespace IdentityPrvd.Infrastructure.Caching;

public static class InMemoryCacheAside
{
    private static readonly MemoryCacheEntryOptions DefaultOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public static async Task<T> GetOrCreateAsync<T>(
        this IMemoryCache cache,
        string key,
        Func<CancellationToken, Task<T>> factory,
        MemoryCacheEntryOptions options = null,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(key, out T cachedValue) && cachedValue != null)
        {
            return cachedValue;
        }

        var hasLock = await SemaphoreSlim.WaitAsync(500, cancellationToken);

        if (!hasLock)
        {
            // If we can't get the lock, try to get from cache one more time
            if (cache.TryGetValue(key, out cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }
            // If still not found, execute factory without caching
            return await factory(cancellationToken);
        }

        try
        {
            // Double-check after acquiring lock
            if (cache.TryGetValue(key, out cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory(cancellationToken);

            if (value != null)
            {
                cache.Set(key, value, options ?? DefaultOptions);
            }

            return value;
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public static void RemoveKey(this IMemoryCache cache, string key)
    {
        cache.Remove(key);
    }

    public static void RemoveByPattern(this IMemoryCache cache, string pattern)
    {
        // Note: IMemoryCache doesn't support pattern-based removal natively
        // This is a simplified implementation - for production, consider using a more sophisticated approach
        // or maintaining a list of keys
    }
}
