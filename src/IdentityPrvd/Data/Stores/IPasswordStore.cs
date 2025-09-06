using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IPasswordStore
{
    Task<List<IdentityPassword>> GetUserPasswordsAsync(Ulid userId);
    Task<List<IdentityPassword>> GetAllUserPasswordsAsync(Ulid userId);
    Task<IdentityPassword> GetUserActivePasswordAsync(Ulid userId);
    Task<List<IdentityPassword>> GetUserActivePasswordsAsync(Ulid userId);
    Task<IdentityPassword> AddAsync(IdentityPassword password);
    Task UpdateRangeAsync(IEnumerable<IdentityPassword> passwords);
    Task<IdentityPassword> UpdateAsync(IdentityPassword password);
}

public class EfPasswordStore(IdentityPrvdContext dbContext) : IPasswordStore
{
    public async Task<List<IdentityPassword>> GetUserPasswordsAsync(Ulid userId)
        => await dbContext.Passwords.Where(s => s.UserId == userId).ToListAsync();

    public async Task<IdentityPassword> AddAsync(IdentityPassword password)
    {
        await dbContext.Passwords.AddAsync(password);
        await dbContext.SaveChangesAsync();
        return password;
    }

    public async Task UpdateRangeAsync(IEnumerable<IdentityPassword> passwords)
    {
        if (passwords.All(rt => dbContext.Entry(rt).State is EntityState.Modified or EntityState.Unchanged))
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }

    public async Task<List<IdentityPassword>> GetAllUserPasswordsAsync(Ulid userId) =>
        await dbContext.Passwords.Where(s => s.UserId == userId).ToListAsync();

    public async Task<IdentityPassword> GetUserActivePasswordAsync(Ulid userId) =>
        await dbContext.Passwords.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

    public async Task<List<IdentityPassword>> GetUserActivePasswordsAsync(Ulid userId) =>
        await dbContext.Passwords
        .Where(s => s.UserId == userId && s.IsActive)
        .ToListAsync();

    public async Task<IdentityPassword> UpdateAsync(IdentityPassword password)
    {
        if (dbContext.Entry(password).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return password;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }
}
