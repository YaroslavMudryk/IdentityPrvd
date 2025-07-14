using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.DataAccess;

public class UserRoleRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUserRole> AddAsync(IdentityUserRole userRole)
    {
        await dbContext.UserRoles.AddAsync(userRole);
        await dbContext.SaveChangesAsync();
        return userRole;
    }
}
