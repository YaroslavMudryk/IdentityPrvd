using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IClientClaimStore
{
    Task DeleteByClientIdAsync(Ulid clientId);
    Task CreateAsync(List<IdentityClientClaim> clientClaims);
}

public class EfClientClaimStore(IdentityPrvdContext dbContext) : IClientClaimStore
{
    public async Task CreateAsync(List<IdentityClientClaim> clientClaims)
    {
        await dbContext.ClientClaims.AddRangeAsync(clientClaims);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteByClientIdAsync(Ulid clientId)
    {
        var claims = await dbContext.ClientClaims.Where(s => s.ClientId == clientId).ToListAsync();
        dbContext.ClientClaims.RemoveRange(claims);
        await dbContext.SaveChangesAsync();
    }
}
