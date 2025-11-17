using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IClientSecretStore
{
    Task DeleteByClientIdAsync(Guid clientId);
}

public class EfClientSecretStore(IdentityPrvdContext dbContext) : IClientSecretStore
{
    public async Task DeleteByClientIdAsync(Guid clientId)
    {
        var secrets = await dbContext.ClientSecrets.Where(s => s.ClientId == clientId).ToListAsync();
        dbContext.ClientSecrets.RemoveRange(secrets);
        await dbContext.SaveChangesAsync();
    }
}
