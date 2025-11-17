using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IBansQuery
{
    /// <summary>
    /// Gets the active ban for a user at the specified time
    /// </summary>
    Task<IdentityBan?> GetActiveBanByUserIdAsync(Guid userId, DateTime utcNow);
}

public class EfBansQuery(IdentityPrvdContext dbContext) : IBansQuery
{
    public async Task<IdentityBan?> GetActiveBanByUserIdAsync(Guid userId, DateTime utcNow)
    {
        return await dbContext.Bans
            .AsNoTracking()
            .Where(b => b.UserId == userId 
                && b.Start <= utcNow 
                && b.End > utcNow)
            .OrderByDescending(b => b.Start)
            .FirstOrDefaultAsync();
    }
}
