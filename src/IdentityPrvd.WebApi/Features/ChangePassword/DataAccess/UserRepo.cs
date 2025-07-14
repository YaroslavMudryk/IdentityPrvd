using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.ChangePassword.DataAccess;

public class UserRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityUser> GetUserAsync(Ulid userId) =>
        await dbContext.Users.FindAsync(userId);

    public async Task UpdateAsync(IdentityUser user)
    {
        if (dbContext.Entry(user).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }
}
