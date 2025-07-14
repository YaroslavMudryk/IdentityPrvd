using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Signin.DataAccess;

public interface IUsersQuery
{
    Task<IdentityUser> GetUserByLoginNullableAsync(string login);
}

public class UsersQuery(IdentityPrvdContext dbContext) : IUsersQuery
{
    public async Task<IdentityUser> GetUserByLoginNullableAsync(string login)
    {
        return (await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login))!;
    }
}
