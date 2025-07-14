using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.LinkExternalSignin.DataAccess;

public class UserLoginRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUserLogin> AddAsync(IdentityUserLogin userLogin)
    {
        await dbContext.UserLogins.AddAsync(userLogin);
        await dbContext.SaveChangesAsync();
        return userLogin;
    }

    public async Task<IdentityUserLogin> GetAsync(Ulid userId, string provider) =>
        await dbContext.UserLogins
        .FirstOrDefaultAsync(s => s.UserId == userId && s.Provider == provider);

    public async Task HardDeleteAsync(IdentityUserLogin identityUserLogin)
    {
        dbContext.UserLogins.HardRemove(identityUserLogin);
        await dbContext.SaveChangesAsync();
    }
}
