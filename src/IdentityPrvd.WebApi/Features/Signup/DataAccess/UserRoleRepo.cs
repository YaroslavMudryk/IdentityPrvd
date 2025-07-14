using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.Signup.DataAccess;

public class UserRoleRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUserRole> AddUserRoleAsync(IdentityUserRole userRole)
    {
        await dbContext.UserRoles.AddAsync(userRole);
        await dbContext.SaveChangesAsync();
        return userRole;
    }
}
