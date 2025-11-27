using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IRolePermissionStore
{
    Task<IEnumerable<IdentityRolePermission>> AddRangeAsync(IEnumerable<IdentityRolePermission> rolePermissions);
    Task<List<IdentityRolePermission>> GetRolePermissionsByRoleIdAsync(Guid roleId);
    Task DeleteRangeAsync(IEnumerable<IdentityRolePermission> rolePermissions);
}

public class EfRolePermissionStore(IdentityPrvdContext dbContext) : IRolePermissionStore
{
    public async Task<IEnumerable<IdentityRolePermission>> AddRangeAsync(IEnumerable<IdentityRolePermission> rolePermissions)
    {
        await dbContext.RolePermissions.AddRangeAsync(rolePermissions);
        await dbContext.SaveChangesAsync();
        return rolePermissions;
    }

    public async Task<List<IdentityRolePermission>> GetRolePermissionsByRoleIdAsync(Guid roleId)
    {
        return await dbContext.RolePermissions
            .Where(rc => rc.RoleId == roleId)
            .ToListAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<IdentityRolePermission> rolePermissions)
    {
        dbContext.RolePermissions.HardRemove(rolePermissions);
        await dbContext.SaveChangesAsync();
    }
}
