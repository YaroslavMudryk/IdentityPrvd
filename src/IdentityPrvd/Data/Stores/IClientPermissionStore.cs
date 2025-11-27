using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface IClientPermissionStore
{
    Task DeleteByClientIdAsync(Guid clientId);
    Task CreateAsync(List<IdentityClientPermission> clientPermissions);
}

public class EfClientPermissionStore(IdentityPrvdContext dbContext) : IClientPermissionStore
{
    public async Task CreateAsync(List<IdentityClientPermission> clientPermissions)
    {
        await dbContext.ClientPermissions.AddRangeAsync(clientPermissions);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteByClientIdAsync(Guid clientId)
    {
        var permissions = await dbContext.ClientPermissions.Where(s => s.ClientId == clientId).ToListAsync();
        dbContext.ClientPermissions.RemoveRange(permissions);
        await dbContext.SaveChangesAsync();
    }
}
