using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IRefreshTokensQuery
{
    Task<IdentityRefreshToken> GetRefreshTokenWithSessionNullableAsync(string refreshToken);
}

public class EfRefreshTokensQuery(IdentityPrvdContext dbContext) : IRefreshTokensQuery
{
    public async Task<IdentityRefreshToken> GetRefreshTokenWithSessionNullableAsync(string refreshToken)
    {
        return await dbContext.RefreshTokens.AsNoTracking().Include(s => s.Session).Where(s => s.Value == refreshToken).FirstOrDefaultAsync();
    }
}
