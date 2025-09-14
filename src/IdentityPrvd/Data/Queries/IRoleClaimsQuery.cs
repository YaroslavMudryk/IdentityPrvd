using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IRoleClaimsQuery
{
    Task<IReadOnlyList<IdentityClaim>> GetClaimsByUserIdAsync(Ulid userId);
}

public class EfRoleClaimsQuery(IdentityPrvdContext dbContext) : IRoleClaimsQuery
{
    public async Task<IReadOnlyList<IdentityClaim>> GetClaimsByUserIdAsync(Ulid userId)
    {
        var userRoleIds = await dbContext.UserRoles.AsNoTracking().Where(s => s.UserId == userId).Select(s => s.RoleId).ToListAsync();
        return await dbContext.RoleClaims.AsNoTracking().Where(s => userRoleIds.Contains(s.RoleId)).Select(s => s.Claim).ToListAsync();
    }
}
