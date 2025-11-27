using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.Data.Stores;

public interface IPermissionStore
{
    Task<IdentityPermission> AddAsync(IdentityPermission permission);
    Task<IdentityPermission> UpdateAsync(IdentityPermission permission);
    Task<IdentityPermission> GetAsync(Guid permissionId);
    Task DeleteAsync(IdentityPermission permission);
    Task<List<IdentityClientPermission>> GetClientPermissionsByIdAsync(Guid permissionId);
    Task DeleteClientPermissionsAsync(IEnumerable<IdentityClientPermission> clientPermissionsToDelete);
    Task<List<IdentityRolePermission>> GetRolePermissionsByIdAsync(Guid permissionId);
    Task DeleteRolePermissionsAsync(IEnumerable<IdentityRolePermission> rolePermissionsToDelete);
}

public class EfPermissionStore(IdentityPrvdContext dbContext) : IPermissionStore
{
    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
                await dbContext.Database.BeginTransactionAsync();

    public async Task<IdentityPermission> AddAsync(IdentityPermission permission)
    {
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<IdentityPermission> UpdateAsync(IdentityPermission permission)
    {
        if (dbContext.Entry(permission).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return permission;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

    public async Task<IdentityPermission> GetAsync(Guid permissionId) =>
        await dbContext.Permissions.Where(s => s.Id == permissionId).FirstOrDefaultAsync() ?? throw new NotFoundException($"Permission with id:{permissionId} not found");

    public async Task DeleteAsync(IdentityPermission permission)
    {
        dbContext.Permissions.HardRemove(permission);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<IdentityClientPermission>> GetClientPermissionsByIdAsync(Guid permissionId) =>
        await dbContext.ClientPermissions.Where(cp => cp.PermissionId == permissionId).ToListAsync();

    public async Task DeleteClientPermissionsAsync(IEnumerable<IdentityClientPermission> clientPermissionsToDelete)
    {
        dbContext.ClientPermissions.HardRemove(clientPermissionsToDelete);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<IdentityRolePermission>> GetRolePermissionsByIdAsync(Guid permissionId) =>
        await dbContext.RolePermissions.Where(rc => rc.PermissionId == permissionId).ToListAsync();

    public async Task DeleteRolePermissionsAsync(IEnumerable<IdentityRolePermission> rolePermissionsToDelete)
    {
        dbContext.RolePermissions.HardRemove(rolePermissionsToDelete);
        await dbContext.SaveChangesAsync();
    }
}
