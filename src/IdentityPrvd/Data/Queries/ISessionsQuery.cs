using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Security.Sessions.GetSessions.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface ISessionsQuery
{
    Task<IReadOnlyList<SessionDto>> GetActiveUserSessionsAsync(Guid userId);
    Task<IReadOnlyCollection<IdentitySession>> GetAllActiveSessionsAsync();
    Task<IdentitySession> GetSessionAsync(Guid sessionId);
}

public class EfSessionsQuery(IdentityPrvdContext dbContext) : ISessionsQuery
{
    public async Task<IReadOnlyList<SessionDto>> GetActiveUserSessionsAsync(Guid userId)
    {
        return await dbContext.Sessions
            .Where(s => s.UserId == userId && (s.Status == SessionStatus.Active || s.Status == SessionStatus.New))
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => s.MapDto())
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IdentitySession>> GetAllActiveSessionsAsync() =>
        await dbContext.Sessions.AsNoTracking().Where(s => s.Status == SessionStatus.Active).ToListAsync();

    public async Task<IdentitySession> GetSessionAsync(Guid sessionId) =>
        await dbContext.Sessions.AsNoTracking().Where(s => s.Id == sessionId).FirstOrDefaultAsync();
}
