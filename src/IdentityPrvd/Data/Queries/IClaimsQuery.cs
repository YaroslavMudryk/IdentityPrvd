using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Features.Authorization.Claims.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IClaimsQuery
{
    Task<IReadOnlyList<ClaimDto>> GetClaimsAsync();
    Task<ClaimDto> GetClaimAsync(Guid claimId);
    Task<int> GetRolesCountByClaimIdAsync(Guid claimId);
    Task<int> GetClientsCountByClaimIdAsync(Guid claimId);
    Task<List<IdentityClaim>> GetClaimsByIdsAsync(Guid[] claimIds);
    Task<IdentityClaim> GetClaimByTypeAndValueAsync(string type, string value);
    Task<bool> IsExistsClaimAsync();
}

public class EfClaimsQuery(IdentityPrvdContext dbContext) : IClaimsQuery
{
    public async Task<IReadOnlyList<ClaimDto>> GetClaimsAsync()
    {
        return await dbContext.Claims
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ProjectToDto()
            .ToListAsync();
    }

    public async Task<ClaimDto> GetClaimAsync(Guid claimId)
    {
        var claim = await dbContext.Claims
            .Where(s => s.Id == claimId)
            .ProjectToDto()
            .FirstOrDefaultAsync() ?? throw new NotFoundException($"Claim with id:{claimId} not found");

        claim.RolesCount = await GetRolesCountByClaimIdAsync(claimId);
        claim.ClientsCount = await GetClientsCountByClaimIdAsync(claimId);

        return claim;
    }

    public async Task<int> GetRolesCountByClaimIdAsync(Guid claimId) =>
        await dbContext.RoleClaims.Where(s => s.ClaimId == claimId).CountAsync();

    public async Task<int> GetClientsCountByClaimIdAsync(Guid claimId) =>
        await dbContext.ClientClaims.Where(s => s.ClaimId == claimId).CountAsync();

    public async Task<IdentityClaim> GetClaimByTypeAndValueAsync(string type, string value) =>
        await dbContext.Claims.AsNoTracking().Where(s => s.Type == type && s.Value == value).FirstOrDefaultAsync();

    public async Task<List<IdentityClaim>> GetClaimsByIdsAsync(Guid[] claimIds) =>
        await dbContext.Claims.AsNoTracking().Where(s => claimIds.Contains(s.Id)).ToListAsync();

    public async Task<bool> IsExistsClaimAsync() =>
        await dbContext.Claims.AsNoTracking().AnyAsync();
}
