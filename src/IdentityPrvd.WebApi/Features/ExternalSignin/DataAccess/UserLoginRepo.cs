using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.DataAccess;

public class UserLoginRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUserLogin> AddAsync(IdentityUserLogin userLogin)
    {
        await dbContext.UserLogins.AddAsync(userLogin);
        await dbContext.SaveChangesAsync();
        return userLogin;
    }
}
