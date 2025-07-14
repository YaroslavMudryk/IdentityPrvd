using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions.DataAccess;
using IdentityPrvd.WebApi.Features.Sessions.GetSessions.Dtos;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.ServerSideSessions;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Sessions.GetSessions.Services;

public class GetSessionsOrchestrator(
    IUserContext userContext,
    ISessionManager sessionManager,
    GetSessionsQuery sessionsQuery)
{
    public async Task<IReadOnlyList<SessionDto>> GetUserSessionsAsync()
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        var dbSessions = await sessionsQuery.GetActiveUserSessionsAsync(Ulid.Parse(currentUser.UserId));
        var cacheSessions = await sessionManager.GetUserSessionsAsync(currentUser.UserId);

        return SortAndSyncSessionsData([.. dbSessions], cacheSessions, currentUser.SessionId);
    }

    private static IReadOnlyList<SessionDto> SortAndSyncSessionsData(List<SessionDto> dbSessions, IList<SessionInfo> cacheSessions, string currentSessionId)
    {
        var cacheLookup = cacheSessions.ToDictionary(s => s.SessionId);
        foreach (var session in dbSessions)
        {
            if (cacheLookup.TryGetValue(session.Id.GetIdAsString(), out var cacheSession))
            {
                session.LastActivityAt = cacheSession.LastAccessedAt;
            }
        }

        return dbSessions
            .OrderByDescending(s => s.Id == currentSessionId.GetIdAsUlid())
            .ThenByDescending(s => s.LastActivityAt)
            .ToList().AsReadOnly();
    }
}
