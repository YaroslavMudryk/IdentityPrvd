using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Features.Claims.Dtos;
using IdentityPrvd.WebApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Claims.DataAccess;

public class ClaimsQuery(IdentityPrvdContext dbContext)
{
    public async Task<IReadOnlyList<ClaimDto>> GetClaimsAsync()
    {
        return await dbContext.Claims
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ProjectToDto()
            .ToListAsync();
    }

    public async Task<ClaimDto> GetClaimAsync(Ulid claimId)
    {
        var claim = await dbContext.Claims
            .Where(s => s.Id == claimId)
            .ProjectToDto()
            .FirstOrDefaultAsync() ?? throw new NotFoundException($"Claim with id:{claimId} not found");

        claim.RolesCount = await GetRolesCountByClaimIdAsync(claimId);
        claim.ClientsCount = await GetClientsCountByClaimIdAsync(claimId);

        return claim;
    }

    private async Task<int> GetRolesCountByClaimIdAsync(Ulid claimId) =>
        await dbContext.RoleClaims.Where(s => s.ClaimId == claimId).CountAsync();

    private async Task<int> GetClientsCountByClaimIdAsync(Ulid claimId) =>
        await dbContext.ClientClaims.Where(s => s.ClaimId == claimId).CountAsync();
}
