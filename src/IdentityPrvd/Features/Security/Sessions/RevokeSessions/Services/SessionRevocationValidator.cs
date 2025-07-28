using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Domain.Entities;
using IdentityPrvd.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace IdentityPrvd.Features.Security.Sessions.RevokeSessions.Services;

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
