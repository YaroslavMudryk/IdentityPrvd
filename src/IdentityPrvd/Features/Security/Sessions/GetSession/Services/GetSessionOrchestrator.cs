using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Features.Security.Sessions.GetSession.Dtos;
using IdentityPrvd.Mappers;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Security.Sessions.GetSession.Services;

public class GetSessionOrchestrator(
    IIdentityContext identityContext,
    ISessionManager sessionManager,
    ISessionsQuery sessionsQuery)
{
    public async Task<SessionDetailDto> GetUserSessionAsync(Guid sessionId)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermission(IdentityPermissions.Sessions.Read);

        var dbSession = await sessionsQuery.GetSessionAsync(sessionId);

        if (dbSession.UserId.GetIdAsString() != currentUser.UserId)
            throw new UnauthorizedException();

        var session = dbSession.MapToDto();

        var cacheSession = await sessionManager.GetUserSessionAsync(sessionId.GetIdAsString());
        session.LastActivityAt = cacheSession?.LastAccessedAt;

        return session;
    }
}
