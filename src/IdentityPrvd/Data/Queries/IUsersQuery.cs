using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IUsersQuery
{
    Task<bool> IsExistUserByLoginAsync(string login);
    Task<bool> IsExistUserByUserNameAsync(string userName);
    Task<IdentityUser> GetUserByLoginNullableAsync(string login);
}

public class EfUsersQuery(IdentityPrvdContext dbContext) : IUsersQuery
{
    public async Task<bool> IsExistUserByLoginAsync(string login)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login);
        return user != null;
    }

    public async Task<bool> IsExistUserByUserNameAsync(string userName)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == userName);
        return user != null;
    }

    public async Task<IdentityUser> GetUserByLoginNullableAsync(string login)
    {
        return (await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Login == login))!;
    }
}
