using IdentityPrvd.WebApi.Db;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Signup.DataAccess;

public interface IUserValidatorQuery
{
    Task<bool> IsExistUserByLoginAsync(string login);
    Task<bool> IsExistUserByUserNameAsync(string userName);
}

public class UserValidatorQuery(IdentityPrvdContext dbContext) : IUserValidatorQuery
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
}
