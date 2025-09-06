using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IConfirmStore
{
    Task<IdentityConfirm> AddAsync(IdentityConfirm confirm);
    Task<IdentityConfirm> UpdateAsync(IdentityConfirm confirm);
    Task<IdentityConfirm> GetConfirmByCodeAsync(string verifyId);
}

public class EfConfirmStore(IdentityPrvdContext dbContext) : IConfirmStore
{
    public async Task<IdentityConfirm> AddAsync(IdentityConfirm confirm)
    {
        await dbContext.Confirms.AddAsync(confirm);
        await dbContext.SaveChangesAsync();
        return confirm;
    }

    public async Task<IdentityConfirm> UpdateAsync(IdentityConfirm confirm)
    {
        if (dbContext.Entry(confirm).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return confirm;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }

    public async Task<IdentityConfirm> GetConfirmByCodeAsync(string code)
        => await dbContext.Confirms.FirstOrDefaultAsync(s => s.Code == code);
}
