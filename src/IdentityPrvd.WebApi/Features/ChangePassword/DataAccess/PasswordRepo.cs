using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.ChangePassword.DataAccess;

public class PasswordRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await dbContext.Database.BeginTransactionAsync();

    public async Task<IdentityPassword> AddAsync(IdentityPassword password)
    {
        await dbContext.Passwords.AddAsync(password);
        await dbContext.SaveChangesAsync();
        return password;
    }

    public async Task<List<IdentityPassword>> GetAllUserPasswordsAsync(Ulid userId) =>
        await dbContext.Passwords.Where(s => s.UserId == userId).ToListAsync();

    public async Task<IdentityPassword> GetUserActivePasswordAsync(Ulid userId) =>
        await dbContext.Passwords.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

    public async Task<List<IdentityPassword>> GetUserActivePasswordsAsync(Ulid userId) =>
        await dbContext.Passwords
        .Where(s => s.UserId == userId && s.IsActive)
        .ToListAsync();
}
