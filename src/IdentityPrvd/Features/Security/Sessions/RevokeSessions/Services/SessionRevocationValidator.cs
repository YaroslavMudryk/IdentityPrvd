using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Domain.Entities;

namespace IdentityPrvd.Features.Security.Sessions.RevokeSessions.Services;

public interface ISessionRevocationValidator
{
    Task EnsureRevocationAllowedAsync(List<IdentitySession> sessions);
}

public class SessionRevocationValidator(
    IIdentityContext identityContext,
    ISessionsQuery sessionsQuery,
    TimeProvider timeProvider) : ISessionRevocationValidator
{
    public async Task EnsureRevocationAllowedAsync(List<IdentitySession> sessions)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        var currentSession = await sessionsQuery.GetSessionAsync(currentUser.SessionId.GetIdAsGuid());

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var isCurrentSessionYoungerThanDay = utcNow - currentSession.CreatedAt < TimeSpan.FromDays(1);
        var hasAnySessionOlderThanCurrent = sessions.Any(s => s.CreatedAt < currentSession.CreatedAt);

        if (isCurrentSessionYoungerThanDay && hasAnySessionOlderThanCurrent)
            throw new BadRequestException("You cannot revoke sessions older than your current one until 24 hours have passed since it was created.");
    }
}
