using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Claims.DataAccess;

public interface IClaimsValidatorQuery
{
    Task<IdentityClaim> GetClaimByTypeAndValueAsync(string type, string value);
}

public class ClaimsValidatorQuery(IdentityPrvdContext dbContext) : IClaimsValidatorQuery
{
    public async Task<IdentityClaim> GetClaimByTypeAndValueAsync(string type, string value) =>
        await dbContext.Claims.AsNoTracking().Where(s => s.Type == type && s.Value == value).FirstOrDefaultAsync();
}
