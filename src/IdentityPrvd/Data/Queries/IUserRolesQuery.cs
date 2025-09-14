using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IUserRolesQuery
{
    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Ulid userId);
}

public class EfUserRolesQuery(IdentityPrvdContext dbContext) : IUserRolesQuery
{
    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Ulid userId) =>
        await dbContext.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.Role.Name).ToListAsync();
}
