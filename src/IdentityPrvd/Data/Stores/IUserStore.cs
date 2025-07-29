using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IUserStore
{
    Task<IdentityUser> GetUserAysnc(Ulid userId);
    Task<IdentityUser> AddAsync(IdentityUser user);
    Task<IdentityUser> UpdateAsync(IdentityUser user);
}

public class EfUserStore(IdentityPrvdContext dbContext) : IUserStore
{
    public async Task<IdentityUser> GetUserAysnc(Ulid userId)
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
}
