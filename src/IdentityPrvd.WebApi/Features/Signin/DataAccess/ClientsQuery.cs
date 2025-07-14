using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Signin.DataAccess;

public interface IClientsQuery
{
    Task<IdentityClient> GetClientByIdNullableAsync(string clientId);
    Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId);
}

public class ClientsQuery(IdentityPrvdContext dbContext) : IClientsQuery
{
    public async Task<IdentityClient> GetClientByIdNullableAsync(string clientId)
    {
        return (await dbContext.Clients.AsNoTracking().Where(c => c.ClientId == clientId).FirstOrDefaultAsync())!;
    }

    public async Task<IdentityClientSecret> GetClientSecretNullableAsync(string clientId)
    {
        return (await dbContext.ClientSecrets.AsNoTracking().Where(c => c.ClientId == clientId.GetIdAsUlid()).FirstOrDefaultAsync())!;
    }
}
