using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.Signup.DataAccess;

public class ConfirmRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentityConfirm> AddAsync(IdentityConfirm confirm)
    {
        await dbContext.Confirms.AddAsync(confirm);
        await dbContext.SaveChangesAsync();
        return confirm;
    }
}
