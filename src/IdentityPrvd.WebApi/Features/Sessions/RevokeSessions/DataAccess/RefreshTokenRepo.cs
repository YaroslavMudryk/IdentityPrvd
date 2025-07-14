using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.DataAccess;

public class RefreshTokenRepo(IdentityPrvdContext dbContext)
{
    public async Task<List<IdentityRefreshToken>> GetRefreshTokensBySessionIdsAsync(Ulid[] sessionIds)
    {
        return await dbContext.RefreshTokens
            .Where(s => sessionIds.Contains(s.SessionId) && s.UsedAt == null)
            .ToListAsync();
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
}
