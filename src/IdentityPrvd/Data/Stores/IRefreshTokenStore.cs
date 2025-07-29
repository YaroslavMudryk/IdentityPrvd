using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IRefreshTokenStore
{
    Task<List<IdentityRefreshToken>> GetRefreshTokensBySessionIdAsync(Ulid sessionId);
    Task<List<IdentityRefreshToken>> GetRefreshTokensBySessionIdsAsync(Ulid[] sessionIds);
    Task<IdentityRefreshToken> AddAsync(IdentityRefreshToken refreshToken);
    Task<IdentityRefreshToken> UpdateAsync(IdentityRefreshToken refreshToken);
    Task UpdateRangeAsync(List<IdentityRefreshToken> refreshTokens);
}

public class EfRefreshTokenStore(IdentityPrvdContext dbContext) : IRefreshTokenStore
{
    public async Task<List<IdentityRefreshToken>> GetRefreshTokensBySessionIdAsync(Ulid sessionId)
    {
        return await dbContext.RefreshTokens
            .Where(s => s.SessionId == sessionId && s.UsedAt == null)
            .ToListAsync();
    }

    public async Task<IdentityRefreshToken> AddAsync(IdentityRefreshToken refreshToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<IdentityRefreshToken> UpdateAsync(IdentityRefreshToken refreshToken)
    {
        if (dbContext.Entry(refreshToken).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return refreshToken;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

    public async Task UpdateRangeAsync(List<IdentityRefreshToken> refreshTokens)
    {
        if (refreshTokens.All(rt => dbContext.Entry(rt).State is EntityState.Modified or EntityState.Unchanged))
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }


    public async Task<List<IdentityRefreshToken>> GetRefreshTokensBySessionIdsAsync(Ulid[] sessionIds)
    {
        return await dbContext.RefreshTokens
            .Where(s => sessionIds.Contains(s.SessionId) && s.UsedAt == null)
            .ToListAsync();
    }
}
