using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.ChangePassword.DataAccess;

public class SessionRepo(IdentityPrvdContext dbContext)
{
    public async Task<IdentitySession> GetAsync(Ulid sessionId)
    {
        return await dbContext.Sessions.Where(s => s.Id == sessionId && s.Status != SessionStatus.Close).FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Session with id:{sessionId} not found");
    }

    public async Task<List<IdentitySession>> GetActiveSessionsByUserIdAsync(Ulid userId) =>
        await dbContext.Sessions.Where(s => s.UserId == userId && s.Status != SessionStatus.Close).ToListAsync();

    public async Task<IdentitySession> UpdateAsync(IdentitySession session)
    {
        if (dbContext.Entry(session).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return session;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }
}
