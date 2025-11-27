using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IRolePermissionsQuery
{
    Task<IReadOnlyList<IdentityPermission>> GetPermissionsByUserIdAsync(Guid userId);
}

public class EfRolePermissionsQuery(IdentityPrvdContext dbContext) : IRolePermissionsQuery
{
    public async Task<IReadOnlyList<IdentityPermission>> GetPermissionsByUserIdAsync(Guid userId)
    {
        var userRoleIds = await dbContext.UserRoles.AsNoTracking().Where(s => s.UserId == userId).Select(s => s.RoleId).ToListAsync();
        return await dbContext.RolePermissions.AsNoTracking().Where(s => userRoleIds.Contains(s.RoleId)).Select(s => s.Permission).ToListAsync();
    }
}
