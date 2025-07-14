using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.Signup.DataAccess;

public class PasswordRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityPassword> AddAsync(IdentityPassword password)
    {
        await dbContext.Passwords.AddAsync(password);
        await dbContext.SaveChangesAsync();
        return password;
    }
}
