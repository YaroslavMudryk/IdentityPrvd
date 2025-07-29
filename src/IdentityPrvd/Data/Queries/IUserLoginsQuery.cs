using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IUserLoginsQuery
{
    Task<List<IdentityUserLogin>> GetUserLoginsAsync(Ulid userId);
    Task<IdentityUserLogin> GetUserLoginByProviderAsync(string userId, string provider);
    Task<IdentityUserLogin> GetUserLoginByProviderWithUserAsync(string userId, string provider);
}

public class EfUserLoginsQuery(IdentityPrvdContext dbContext) : IUserLoginsQuery
{
    public async Task<List<IdentityUserLogin>> GetUserLoginsAsync(Ulid userId) =>
        await dbContext.UserLogins
        .AsNoTracking()
        .Where(s => s.UserId == userId)
        .ToListAsync();

    public async Task<IdentityUserLogin> GetUserLoginByProviderAsync(string userId, string provider) =>
        await dbContext.UserLogins
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Provider == provider && s.ProviderUserId == userId);

    public async Task<IdentityUserLogin> GetUserLoginByProviderWithUserAsync(string userId, string provider) =>
        await dbContext.UserLogins
        .AsNoTracking()
        .Include(s => s.User)
        .FirstOrDefaultAsync(s => s.Provider == provider && s.ProviderUserId == userId);
}
