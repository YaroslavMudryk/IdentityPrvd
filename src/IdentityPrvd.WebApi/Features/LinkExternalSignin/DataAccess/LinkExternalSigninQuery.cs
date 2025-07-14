using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.DataAccess;

public class LinkExternalSigninQuery(IdentityPrvdContext dbContext)
{
    public async Task<List<IdentityUserLogin>> GetUserLoginsAsync(Ulid userId) =>
        await dbContext.UserLogins
        .AsNoTracking()
        .Where(s => s.UserId == userId)
        .ToListAsync();

    public async Task<IdentityUserLogin> GetUserLoginByProviderAsync(string userId, string provider) =>
        await dbContext.UserLogins
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Provider == provider && s.ProviderUserId == userId);
}
