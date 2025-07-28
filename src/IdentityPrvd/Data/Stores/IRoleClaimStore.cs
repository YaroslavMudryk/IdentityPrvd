using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IRoleClaimStore
{
    Task<IEnumerable<IdentityRoleClaim>> AddRangeAsync(IEnumerable<IdentityRoleClaim> roleClaims);
    Task<List<IdentityRoleClaim>> GetRoleClaimsByRoleIdAsync(Ulid roleId);
    Task DeleteRangeAsync(IEnumerable<IdentityRoleClaim> roleClaims);
}

public class EfRoleClaimStore(IdentityPrvdContext dbContext) : IRoleClaimStore
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
