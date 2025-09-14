using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IUserStore
{
    Task<IdentityUser> GetUserAsync(Ulid userId);
    Task<IdentityUser> GetUserByLoginAsync(string login);
    Task<IdentityUser> AddAsync(IdentityUser user);
    Task<IdentityUser> UpdateAsync(IdentityUser user);
    Task<IdentityUser> UpdateAsync(IdentityUser user, bool attached = false);
}

public class EfUserStore(IdentityPrvdContext dbContext) : IUserStore
{
    public async Task<IdentityUser> GetUserAsync(Ulid userId)
        => await dbContext.Users.FindAsync(userId);

    public async Task<IdentityUser> AddAsync(IdentityUser user)
    {
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<IdentityUser> UpdateAsync(IdentityUser user)
    {
        if (dbContext.Entry(user).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return user;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

    public async Task<IdentityUser> UpdateAsync(IdentityUser user, bool attach = false)
    {
        if (attach)
            dbContext.Users.Attach(user);
        return await UpdateAsync(user);
    }

    public async Task<IdentityUser> GetUserByLoginAsync(string login)
        => await dbContext.Users.FirstOrDefaultAsync(s => s.Login == login);
}
