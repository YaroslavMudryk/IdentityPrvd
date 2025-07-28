using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Stores;

public interface ISessionStore
{
    Task<IdentitySession> GetAsync(Ulid sessionId);
    Task<List<IdentitySession>> GetActiveSessionsByUserIdAsync(Ulid userId);
    Task<List<IdentitySession>> GetActiveSessionsByIdsAsync(Ulid[] sessionIds);
    Task<IdentitySession> AddAsync(IdentitySession session);
    Task<IdentitySession> UpdateAsync(IdentitySession session);
    Task UpdateRangeAsync(List<IdentitySession> sessions);
}

public class EfSessionStore(IdentityPrvdContext dbContext) : ISessionStore
{
    public async Task<IdentitySession> GetAsync(Ulid sessionId)
    {
        return await dbContext.Sessions.Where(s => s.Id == sessionId && s.Status != SessionStatus.Close).FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Session with id:{sessionId} not found");
    }

    public async Task<List<IdentitySession>> GetActiveSessionsByUserIdAsync(Ulid userId) =>
        await dbContext.Sessions.Where(s => s.UserId == userId && s.Status != SessionStatus.Close).ToListAsync();

    public async Task<IdentitySession> AddAsync(IdentitySession session)
    {
        await dbContext.Sessions.AddAsync(session);
        await dbContext.SaveChangesAsync();
        return session;
    }

    public async Task<IdentitySession> UpdateAsync(IdentitySession session)
    {
        if (dbContext.Entry(session).State is EntityState.Modified or EntityState.Unchanged)
        {
            await dbContext.SaveChangesAsync();
            return session;
        }

        throw new ArgumentException("Entity must be in modified state or unchanged state to be updated.");
    }

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
