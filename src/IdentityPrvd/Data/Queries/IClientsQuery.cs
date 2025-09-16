using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface IClientsQuery
{
    Task<IdentityClient> GetClientByIdAsync(string clientId);
    Task<IdentityClientSecret> GetClientSecretAsync(string clientId);
    Task<IdentityClient> GetClientByIdNullableAsync(string clientId);
    Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId);
    Task<bool> IsExistsClientAsync();
}

public class EfClientsQuery(IdentityPrvdContext dbContext) : IClientsQuery
{
    public async Task<IdentityClientSecret> GetClientSecretAsync(string id)
    {
        return await dbContext.ClientSecrets.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive && s.ClientId == id.GetIdAsUlid());
    }

    public async Task<IdentityClient> GetClientByIdNullableAsync(string clientId)
    {
        return (await dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.ClientId == clientId))!;
    }

    public async Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId)
    {
        return (await dbContext.ClientSecrets.AsNoTracking().Where(c => c.ClientId == clientId.GetIdAsUlid()).FirstOrDefaultAsync())!;
    }

    public async Task<IdentityClient> GetClientByIdAsync(string clientId) =>
        await dbContext.Clients
        .AsNoTracking()
        .SingleOrDefaultAsync(s => s.ClientId == clientId);

    public async Task<bool> IsExistsClientAsync() =>
        await dbContext.Clients.AsNoTracking().AnyAsync();
}
