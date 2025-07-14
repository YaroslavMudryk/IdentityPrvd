using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.RefreshToken.DataAccess;

public class RefreshTokenRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await dbContext.Database.BeginTransactionAsync();

    public async Task<IdentityRefreshToken> AddAsync(IdentityRefreshToken refreshToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<IdentityRefreshToken> UpdateAsync(IdentityRefreshToken refreshToken)
    {
        dbContext.RefreshTokens.Update(refreshToken);
        await dbContext.SaveChangesAsync();
        return refreshToken;
    }
}
