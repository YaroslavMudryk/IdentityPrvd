using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Roles.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IRolesQuery
{
    Task<Ulid> GetDefaultRoleIdAsync();
    Task<IdentityRole> GetDefaultRoleAsync();
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(bool withStats = true);
    Task<RoleDto> GetRoleAsync(Ulid roleId);
    Task<IdentityRole> GetRoleByNameAsync(string name);
    Task<IdentityRole> GetRoleByIdAsync(Ulid roleId);
    Task<int> GetUsersCountByRoleIdAsync(Ulid roleId);
    Task<int> GetClaimsCountByRoleIdAsync(Ulid roleId);
    Task<bool> IsExistsRoleAsync();
}

public class EfRolesQuery(IdentityPrvdContext dbContext) : IRolesQuery
{
    public async Task<Ulid> GetDefaultRoleIdAsync()
    {
        var defaultRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(s => s.IsDefault);
        return defaultRole!.Id;
    }

    public async Task<IdentityRole> GetDefaultRoleAsync() =>
        await dbContext.Roles
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.IsDefault);

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

    public async Task<int> GetUsersCountByRoleIdAsync(Ulid roleId) =>
        await dbContext.UserRoles.Where(s => s.RoleId == roleId).CountAsync();

    public async Task<int> GetClaimsCountByRoleIdAsync(Ulid roleId) =>
        await dbContext.RoleClaims.Where(s => s.RoleId == roleId).CountAsync();

    public async Task<IdentityRole> GetRoleByIdAsync(Ulid roleId) =>
        await dbContext.Roles.Where(s => s.Id == roleId).FirstOrDefaultAsync();

    public async Task<IdentityRole> GetRoleByNameAsync(string name) =>
        await dbContext.Roles.AsNoTracking().Where(s => s.NameNormalized == name).FirstOrDefaultAsync();

    public async Task<bool> IsExistsRoleAsync() =>
        await dbContext.Roles.AsNoTracking().AnyAsync();
}
