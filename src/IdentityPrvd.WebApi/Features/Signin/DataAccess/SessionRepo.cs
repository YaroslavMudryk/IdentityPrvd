using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.Signin.DataAccess;

public class SessionRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await dbContext.Database.BeginTransactionAsync();

    public async Task<IdentitySession> AddAsync(IdentitySession session)
    {
        await dbContext.Sessions.AddAsync(session);
        await dbContext.SaveChangesAsync();
        return session;
    }
}
