using IdentityPrvd.WebApi.Db;
using IdentityPrvd.WebApi.Db.Entities;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.UserContext;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.Services;

public class SessionRevocationValidator(
    IUserContext userContext,
    TimeProvider timeProvider,
    IdentityPrvdContext dbContext)
{
    public async Task EnsureRevocationAllowedAsync(List<IdentitySession> sessions)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        var currentSession = await dbContext.Sessions.AsNoTracking().Where(s => s.Id == currentUser.SessionId.GetIdAsUlid()).FirstOrDefaultAsync();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var isCurrentSessionYoungerThanDay = utcNow - currentSession.CreatedAt < TimeSpan.FromDays(1);
        var hasAnySessionOlderThanCurrent = sessions.Any(s => s.CreatedAt < currentSession.CreatedAt);

        if (isCurrentSessionYoungerThanDay && hasAnySessionOlderThanCurrent)
            throw new BadRequestException("You cannot revoke sessions older than your current one until 24 hours have passed since it was created.");
    }
}
