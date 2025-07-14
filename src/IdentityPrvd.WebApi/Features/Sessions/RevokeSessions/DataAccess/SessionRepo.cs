using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.DataAccess;

public class SessionRepo(IdentityPrvdContext dbContext)
{
    public async Task<IDbContextTransaction> BeginTransactionAsync() =>
        await dbContext.Database.BeginTransactionAsync();

    public async Task<List<IdentitySession>> GetActiveSessionsByIdsAsync(Ulid[] sessionIds)
    {
        return await dbContext.Sessions.Where(s => sessionIds.Contains(s.Id) && s.Status != SessionStatus.Close).ToListAsync();
    }

    public async Task UpdateRangeAsync(List<IdentitySession> sessions)
    {
        if (sessions.All(s => dbContext.Entry(s).State is EntityState.Modified or EntityState.Unchanged))
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        throw new ArgumentException("Entities must be in modified state or unchanged state to be updated.");
    }
}
