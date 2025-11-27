using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityPrvd.Data.Queries;

public class CachedClientPermissionsQuery(
    IClientPermissionsQuery inner,
    IMemoryCache cache) : IClientPermissionsQuery
{
    private const string CacheKeyPrefixPermissions = "client_permissions_";
    private const string CacheKeyPrefixPermissionIds = "client_permission_ids_";

    public async Task<IReadOnlyList<IdentityPermission>> GetPermissionsByClientIdAsync(string clientId)
    {
        var cacheKey = $"{CacheKeyPrefixPermissions}{clientId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetPermissionsByClientIdAsync(clientId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }

    public async Task<IReadOnlyList<string>> GetPermissionsIdsByClientIdAsync(Guid clientId)
    {
        var cacheKey = $"{CacheKeyPrefixPermissionIds}{clientId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetPermissionsIdsByClientIdAsync(clientId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }
}
