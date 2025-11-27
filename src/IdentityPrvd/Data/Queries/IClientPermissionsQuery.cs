using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IClientPermissionsQuery
{
    Task<IReadOnlyList<IdentityPermission>> GetPermissionsByClientIdAsync(string clientId);
    Task<IReadOnlyList<string>> GetPermissionsIdsByClientIdAsync(Guid clientId);
}

public class EfClientPermissionsQuery(IdentityPrvdContext dbContext) : IClientPermissionsQuery
{
    public async Task<IReadOnlyList<IdentityPermission>> GetPermissionsByClientIdAsync(string clientId)
    {
        var userClientId = await dbContext.Clients.AsNoTracking().Where(s => s.ClientId == clientId).Select(s => s.Id).FirstOrDefaultAsync();
        return await dbContext.ClientPermissions.AsNoTracking().Where(s => s.ClientId == userClientId).Select(s => s.Permission).ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetPermissionsIdsByClientIdAsync(Guid clientId) =>
        await dbContext.ClientPermissions.Where(s => s.ClientId == clientId).Select(s => s.PermissionId.GetIdAsString()).ToListAsync();
}
