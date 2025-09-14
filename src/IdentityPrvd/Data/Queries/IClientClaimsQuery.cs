using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IClientClaimsQuery
{
    Task<IReadOnlyList<IdentityClaim>> GetClaimsByClientIdAsync(string clientId);
}

public class EfClientClaimsQuery(IdentityPrvdContext dbContext) : IClientClaimsQuery
{
    public async Task<IReadOnlyList<IdentityClaim>> GetClaimsByClientIdAsync(string clientId)
    {
        var userClientId = await dbContext.Clients.AsNoTracking().Where(s => s.ClientId == clientId).Select(s => s.Id).FirstOrDefaultAsync();
        return await dbContext.ClientClaims.AsNoTracking().Where(s => s.ClientId == userClientId).Select(s => s.Claim).ToListAsync();
    }
}
