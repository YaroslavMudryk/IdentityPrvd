using IdentityPrvd.Contexts;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Features.Security.Sessions.GetSessions.Dtos;
using IdentityPrvd.Infrastructure.Database.Context;
using IdentityPrvd.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Data.Queries;

public interface ISessionsQuery
{
    Task<IReadOnlyList<SessionDto>> GetActiveUserSessionsAsync(Ulid userId);
    Task<IReadOnlyCollection<IdentitySession>> GetAllActiveSessionsAsync();
    Task<IdentitySession> GetSessionAsync(Ulid sessionId);
}

public class EfSessionsQuery(IdentityPrvdContext dbContext) : ISessionsQuery
{
    public async Task<IReadOnlyList<SessionDto>> GetActiveUserSessionsAsync(Ulid userId)
    {
        return await dbContext.Sessions
            .Where(s => s.UserId == userId && (s.Status == SessionStatus.Active || s.Status == SessionStatus.New))
            .OrderByDescending(s => s.CreatedAt)
            .ProjectToDto()
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IdentitySession>> GetAllActiveSessionsAsync() =>
        await dbContext.Sessions.AsNoTracking().Where(s => s.Status == SessionStatus.Active).ToListAsync();

    public async Task<IdentitySession> GetSessionAsync(Ulid sessionId) =>
        await dbContext.Sessions.AsNoTracking().Where(s => s.Id == sessionId).FirstOrDefaultAsync();
}
