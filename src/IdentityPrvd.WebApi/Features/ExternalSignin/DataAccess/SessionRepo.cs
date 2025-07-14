using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;

namespace IdentityPrvd.WebApi.Features.ExternalSignin.DataAccess;

public class SessionRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentitySession> AddAsync(IdentitySession session)
    {
        await dbContext.Sessions.AddAsync(session);
        await dbContext.SaveChangesAsync();
        return session;
    }
}
