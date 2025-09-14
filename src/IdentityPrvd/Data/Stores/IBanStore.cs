using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;

namespace IdentityPrvd.Data.Stores;

public interface IBanStore
{
    Task<IdentityBan> AddAsync(IdentityBan ban);
}

public class EfBanStore(IdentityPrvdContext dbContext) : IBanStore
{
    public async Task<IdentityBan> AddAsync(IdentityBan ban)
    {
        await dbContext.Bans.AddAsync(ban);
        await dbContext.SaveChangesAsync();
        return ban;
    }
}
