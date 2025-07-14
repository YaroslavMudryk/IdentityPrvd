using IdentityPrvd.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Signup.DataAccess;

public class RolesQuery(IdentityPrvdContext dbContext)
{
    public async Task<Ulid> GetDefaultRoleRoleIdAsync()
    {
        var defaultRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(s => s.IsDefault);
        return defaultRole!.Id;
    }
}
