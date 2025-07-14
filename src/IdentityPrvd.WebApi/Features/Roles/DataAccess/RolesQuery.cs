using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Roles.Dtos;
using IdentityPrvd.WebApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Roles.DataAccess;

public class RolesQuery(IdentityPrvdContext dbContext)
{
    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(bool withStats = true)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ProjectToDto()
            .ToListAsync();

        if (withStats)
            foreach (var role in roles)
            {
                role.UsersCount = await GetUsersCountByRoleIdAsync(role.Id);
                role.ClaimsCount = await GetClaimsCountByRoleIdAsync(role.Id);
            }

        return roles;
    }

    public async Task<RoleDto> GetRoleAsync(Ulid roleId)
    {
        var role = await dbContext.Roles
            .Where(s => s.Id == roleId)
            .ProjectToDto()
            .FirstOrDefaultAsync() ?? throw new NotFoundException($"Role with id:{roleId} not found");

        role.UsersCount = await GetUsersCountByRoleIdAsync(roleId);
        role.ClaimsCount = await GetClaimsCountByRoleIdAsync(roleId);

        return role;
    }

    private async Task<int> GetUsersCountByRoleIdAsync(Ulid roleId) =>
        await dbContext.UserRoles.Where(s => s.RoleId == roleId).CountAsync();

    private async Task<int> GetClaimsCountByRoleIdAsync(Ulid roleId) =>
        await dbContext.RoleClaims.Where(s => s.RoleId == roleId).CountAsync();
}
