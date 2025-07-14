using IdentityPrvd.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Roles.Services;

public class DefaultRoleService(IdentityPrvdContext dbContext)
{
    public async Task MakeRoleAsDefaultAsync(Ulid defaultRoleId)
    {
        var allRoles = await dbContext.Roles.ToListAsync();

        foreach (var role in allRoles)
        {
            if (role.Id == defaultRoleId)
                role.IsDefault = true;
            else
                role.IsDefault = false;
        }
    }
}
