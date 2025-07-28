using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;

namespace IdentityPrvd.Data.Stores;

public interface IUserRoleStore
{
    Task<IdentityUserRole> AddAsync(IdentityUserRole userRole);
}

public class EfUserRoleStore(IdentityPrvdContext dbContext) : IUserRoleStore
{
    public async Task<IdentityUserRole> AddAsync(IdentityUserRole userRole)
    {
        await dbContext.UserRoles.AddAsync(userRole);
        await dbContext.SaveChangesAsync();
        return userRole;
    }
}
