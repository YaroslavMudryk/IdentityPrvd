using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.RestorePassword.DataAccess;

public class ConfirmRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await dbContext.Database.BeginTransactionAsync();

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

    public async Task<IdentityConfirm> GetConfirmByVerifyIdAsync(string verifyId)
        => await dbContext.Confirms.FirstOrDefaultAsync(s => s.VerifyId == verifyId);
}

public class PasswordRepo(IdentityPrvdContext dbContext)
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
}

public class UserRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUser> GetUserAysnc(Ulid userId)
        => await dbContext.Users.FindAsync(userId);

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
