using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions.Dtos;
using IdentityPrvd.WebApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Sessions.GetSessions.DataAccess;

public class GetSessionsQuery(
    IdentityPrvdContext dbContext)
{
    public async Task<IReadOnlyList<SessionDto>> GetActiveUserSessionsAsync(Ulid userId)
    {
        return await dbContext.Sessions
            .Where(s => s.UserId == userId && (s.Status == SessionStatus.Active || s.Status == SessionStatus.New))
            .OrderByDescending(s => s.CreatedAt)
            .ProjectToDto()
            .ToListAsync();
    }
}
