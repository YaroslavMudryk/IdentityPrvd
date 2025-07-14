using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.DataAccess;

public interface IExternalSigninValidatorQuery
{
    Task<IdentityClient> GetClientByIdAsync(string id);
    Task<IdentityClientSecret> GetClientSecretAsync(string id);
}

public class ExternalSigninValidatorQuery(IdentityPrvdContext dbContext) : IExternalSigninValidatorQuery
{
    public async Task<IdentityClient> GetClientByIdAsync(string id)
    {
        return await dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(s => s.ClientId == id);
    }

    public async Task<IdentityClientSecret> GetClientSecretAsync(string id)
    {
        return await dbContext.ClientSecrets.AsNoTracking().FirstOrDefaultAsync(s => s.IsActive && s.ClientId == id.GetIdAsUlid());
    }
}
