using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IUserLoginStore
{
    Task<IdentityUserLogin> AddAsync(IdentityUserLogin userLogin);
    Task<IdentityUserLogin> GetAsync(Ulid userId, string provider);
    Task HardDeleteAsync(IdentityUserLogin identityUserLogin);
}

public class EfUserLoginStore(IdentityPrvdContext dbContext) : IUserLoginStore
{
    public async Task<IdentityUserLogin> AddAsync(IdentityUserLogin userLogin)
    {
        await dbContext.UserLogins.AddAsync(userLogin);
        await dbContext.SaveChangesAsync();
        return userLogin;
    }

    public async Task<IdentityUserLogin> GetAsync(Ulid userId, string provider) =>
        await dbContext.UserLogins
        .FirstOrDefaultAsync(s => s.UserId == userId && s.Provider == provider);

    public async Task HardDeleteAsync(IdentityUserLogin identityUserLogin)
    {
        dbContext.UserLogins.HardRemove(identityUserLogin);
        await dbContext.SaveChangesAsync();
    }
}
