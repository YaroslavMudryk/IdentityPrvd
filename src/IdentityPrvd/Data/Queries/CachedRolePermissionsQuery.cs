using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityPrvd.Data.Queries;

public class CachedRolePermissionsQuery(
    IRolePermissionsQuery inner,
    IMemoryCache cache) : IRolePermissionsQuery
{
    private const string CacheKeyPrefix = "role_permissions_";

    public async Task<IReadOnlyList<IdentityPermission>> GetPermissionsByUserIdAsync(Guid userId)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetPermissionsByUserIdAsync(userId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }
}
