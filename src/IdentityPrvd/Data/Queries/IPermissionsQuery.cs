using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Permissions.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IPermissionsQuery
{
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync();
    Task<PermissionDto> GetPermissionAsync(Guid permissionId);
    Task<int> GetRolesCountByPermissionIdAsync(Guid permissionId);
    Task<int> GetClientsCountByPermissionIdAsync(Guid permissionId);
    Task<List<IdentityPermission>> GetPermissionsByIdsAsync(Guid[] permissionIds);
    Task<IdentityPermission> GetPermissionByValueAsync(string value);
    Task<bool> IsExistsPermissionAsync();
}

public class EfPermissionsQuery(IdentityPrvdContext dbContext) : IPermissionsQuery
{
    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync()
    {
        return await dbContext.Permissions
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ProjectToDto()
            .ToListAsync();
    }

    public async Task<PermissionDto> GetPermissionAsync(Guid permissionId)
    {
        var permission = await dbContext.Permissions
            .Where(s => s.Id == permissionId)
            .ProjectToDto()
            .FirstOrDefaultAsync() ?? throw new NotFoundException($"Permission with id:{permissionId} not found");

        permission.RolesCount = await GetRolesCountByPermissionIdAsync(permissionId);
        permission.ClientsCount = await GetClientsCountByPermissionIdAsync(permissionId);

        return permission;
    }

    public async Task<int> GetRolesCountByPermissionIdAsync(Guid permissionId) =>
        await dbContext.RolePermissions.Where(s => s.PermissionId == permissionId).CountAsync();

    public async Task<int> GetClientsCountByPermissionIdAsync(Guid permissionId) =>
        await dbContext.ClientPermissions.Where(s => s.PermissionId == permissionId).CountAsync();

    public async Task<IdentityPermission> GetPermissionByValueAsync(string value) =>
        await dbContext.Permissions.AsNoTracking().Where(s => s.Value == value).FirstOrDefaultAsync();

    public async Task<List<IdentityPermission>> GetPermissionsByIdsAsync(Guid[] permissionIds) =>
        await dbContext.Permissions.AsNoTracking().Where(s => permissionIds.Contains(s.Id)).ToListAsync();

    public async Task<bool> IsExistsPermissionAsync() =>
        await dbContext.Permissions.AsNoTracking().AnyAsync();
}
