using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.DataAccess;

public class ExternalSigninQuery(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUserLogin> GetUserLoginByProviderAsync(string provider, string providerUserId) =>
        await dbContext.UserLogins
        .AsNoTracking()
        .Include(s => s.User)
        .SingleOrDefaultAsync(s => s.Provider == provider && s.ProviderUserId == providerUserId);

    public async Task<IdentityRole> GetDefaultRoleAsync() =>
        await dbContext.Roles
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.IsDefault);

    public async Task<IdentityClient> GetClientByIdAsync(string clientId) =>
        await dbContext.Clients
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.ClientId == clientId);
}
