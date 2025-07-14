using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Roles.DataAccess;

public interface IRolesValidatorQuery
{
    Task<IdentityRole> GetRoleByNameAsync(string name);
    Task<IdentityRole> GetRoleByIdAsync(Ulid roleId);
    Task<List<IdentityClaim>> GetClaimsByIdsAsync(Ulid[] claimIds);
    Task<int> GetUsersCountAssignedToRoleAsync(Ulid roleId);
}

public class RolesValidatorQuery(IdentityPrvdContext dbContext) : IRolesValidatorQuery
{
    public async Task<List<IdentityClaim>> GetClaimsByIdsAsync(Ulid[] claimIds) =>
        await dbContext.Claims.AsNoTracking().Where(s => claimIds.Contains(s.Id)).ToListAsync();

    public async Task<IdentityRole> GetRoleByIdAsync(Ulid roleId) =>
        await dbContext.Roles.Where(s => s.Id == roleId).FirstOrDefaultAsync();

    public async Task<IdentityRole> GetRoleByNameAsync(string name) =>
        await dbContext.Roles.AsNoTracking().Where(s => s.NameNormalized == name).FirstOrDefaultAsync();

    public async Task<int> GetUsersCountAssignedToRoleAsync(Ulid roleId) =>
        await dbContext.UserRoles.Where(s => s.RoleId == roleId).CountAsync();
}
