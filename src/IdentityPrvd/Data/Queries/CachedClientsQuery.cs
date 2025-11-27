using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Clients.Dtos;
using IdentityPrvd.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityPrvd.Data.Queries;

public class CachedClientsQuery(
    IClientsQuery inner,
    IMemoryCache cache) : IClientsQuery
{
    private const string CacheKeyPrefixClient = "client_";
    private const string CacheKeyPrefixClientSecret = "client_secret_";
    private const string CacheKeyClientExists = "client_exists";

    public async Task<IdentityClient> GetClientByIdAsync(string clientId)
    {
        var cacheKey = $"{CacheKeyPrefixClient}{clientId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetClientByIdAsync(clientId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }

    public async Task<IdentityClientSecret> GetClientSecretAsync(string clientId)
    {
        var cacheKey = $"{CacheKeyPrefixClientSecret}{clientId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetClientSecretAsync(clientId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }

    public async Task<IdentityClient> GetClientByIdNullableAsync(string clientId)
    {
        var cacheKey = $"{CacheKeyPrefixClient}nullable_{clientId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetClientByIdNullableAsync(clientId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }

    public async Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId)
    {
        var cacheKey = $"{CacheKeyPrefixClientSecret}nullable_{clientId}";
        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => await inner.GetClientSecretNullableAsync(clientId),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
    }

    public async Task<bool> IsExistsClientAsync()
    {
        return await cache.GetOrCreateAsync(
            CacheKeyClientExists,
            async ct => await inner.IsExistsClientAsync(),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
    }

    public Task<IReadOnlyList<ClientDto>> GetAllClientsAsync()
    {
        // Don't cache list queries as they change frequently
        return inner.GetAllClientsAsync();
    }

    public Task<IReadOnlyList<ClientDto>> GetClientsByCreatorIdAsync(Guid userId)
    {
        // Don't cache list queries as they change frequently
        return inner.GetClientsByCreatorIdAsync(userId);
    }
}
