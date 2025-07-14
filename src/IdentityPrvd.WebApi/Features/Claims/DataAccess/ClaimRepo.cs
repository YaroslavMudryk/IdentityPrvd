using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.Claims.DataAccess;

public class ClaimRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
                await dbContext.Database.BeginTransactionAsync();

    public async Task<IdentityClaim> AddAsync(IdentityClaim claim)
    {
        await dbContext.Claims.AddAsync(claim);
        await dbContext.SaveChangesAsync();
        return claim;
    }

    public async Task<IdentityClaim> UpdateAsync(IdentityClaim claim)
    {
        if (dbContext.Entry(claim).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return claim;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

    public async Task<IdentityClaim> GetAsync(Ulid claimId) =>
        await dbContext.Claims.Where(s => s.Id == claimId).FirstOrDefaultAsync() ?? throw new NotFoundException($"Claim with id:{claimId} not found");

    public async Task DeleteAsync(IdentityClaim claim)
    {
        dbContext.Claims.HardRemove(claim);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<IdentityClientClaim>> GetClientClaimsByIdAsync(Ulid claimId) =>
        await dbContext.ClientClaims.Where(cc => cc.ClaimId == claimId).ToListAsync();

    public async Task DeleteClientClaimsAsync(IEnumerable<IdentityClientClaim> clientClaimsToDelete)
    {
        dbContext.ClientClaims.HardRemove(clientClaimsToDelete);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<IdentityRoleClaim>> GetRoleClaimsByIdAsync(Ulid claimId) =>
        await dbContext.RoleClaims.Where(rc => rc.ClaimId == claimId).ToListAsync();

    public async Task DeleteRoleClaimsAsync(IEnumerable<IdentityRoleClaim> roleClaimsToDelete)
    {
        dbContext.RoleClaims.HardRemove(roleClaimsToDelete);
        await dbContext.SaveChangesAsync();
    }
}
