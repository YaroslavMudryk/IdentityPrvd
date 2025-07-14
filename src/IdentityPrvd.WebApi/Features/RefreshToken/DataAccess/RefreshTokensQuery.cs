using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.RefreshToken.DataAccess;

public interface IRefreshTokensQuery
{
    Task<IdentityRefreshToken> GetRefreshTokenWithSessionNullableAsync(string refreshToken);
}

public class RefreshTokensQuery(IdentityPrvdContext dbContext) : IRefreshTokensQuery
{
    public async Task<IdentityRefreshToken> GetRefreshTokenWithSessionNullableAsync(string refreshToken)
    {
        return await dbContext.RefreshTokens.AsNoTracking().Include(s => s.Session).Where(s => s.Value == refreshToken).FirstOrDefaultAsync();
    }
}
