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
    public async Task<SessionDetailDto> GetUserSessionAsync(Ulid sessionId)
    {
        var currentUser = identityContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        var dbSession = await sessionsQuery.GetSessionAsync(sessionId);

        if (!currentUser.IsInRoles([DefaultsRoles.Admin, DefaultsRoles.SuperAdmin])
            && dbSession.UserId.GetIdAsString() != currentUser.UserId)
            throw new UnauthorizedException("Not your session");

        var session = dbSession.MapToDto();

        var cacheSession = await sessionManager.GetUserSessionAsync(sessionId.GetIdAsString());
        session.LastActivityAt = cacheSession?.LastAccessedAt;

        return session;
    }
}
