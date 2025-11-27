using IdentityPrvd.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityPrvd.Data.Queries;

public class CachedUserRolesQuery(
    IUserRolesQuery inner,
    IMemoryCache cache) : IUserRolesQuery
{
    private const string CacheKeyPrefix = "user_roles_";

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetUserRoleNamesAsync(userId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }
}
