using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Roles.DataAccess;

public class RoleClaimRepo(IdentityPrvdContext dbContext)
{
    public async Task<IEnumerable<IdentityRoleClaim>> AddRangeAsync(IEnumerable<IdentityRoleClaim> roleClaims)
    {
        await dbContext.RoleClaims.AddRangeAsync(roleClaims);
        await dbContext.SaveChangesAsync();
        return roleClaims;
    }

    public async Task<List<IdentityRoleClaim>> GetRoleClaimsByRoleIdAsync(Ulid roleId)
    {
        return await dbContext.RoleClaims
            .Where(rc => rc.RoleId == roleId)
            .ToListAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<IdentityRoleClaim> roleClaims)
    {
        dbContext.RoleClaims.HardRemove(roleClaims);
        await dbContext.SaveChangesAsync();
    }
}
