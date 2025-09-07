using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IConfirmStore
{
    Task<IdentityCode> AddAsync(IdentityCode confirm);
    Task<IdentityCode> UpdateAsync(IdentityCode confirm);
    Task<IdentityCode> GetConfirmByCodeAsync(string verifyId);
}

public class EfConfirmStore(IdentityPrvdContext dbContext) : IConfirmStore
{
    public async Task<IdentityCode> AddAsync(IdentityCode confirm)
    {
        await dbContext.Confirms.AddAsync(confirm);
        await dbContext.SaveChangesAsync();
        return confirm;
    }

    public async Task<IdentityCode> UpdateAsync(IdentityCode confirm)
    {
        if (dbContext.Entry(confirm).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return confirm;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }

    public async Task<IdentityCode> GetConfirmByCodeAsync(string verifyId)
        => await dbContext.Confirms.FirstOrDefaultAsync(s => s.VerifyId == verifyId);
}
