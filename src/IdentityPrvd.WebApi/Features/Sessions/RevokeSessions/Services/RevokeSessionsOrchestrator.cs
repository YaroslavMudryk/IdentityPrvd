using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.DataAccess;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.ServerSideSessions;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Sessions.RevokeSessions.Services;

public class RevokeSessionsOrchestrator(
    IUserContext userContext,
    ISessionManager sessionManager,
    SessionRevocationValidator revocationValidator,
    SessionRepo sessionRepo,
    RefreshTokenRepo refreshTokenRepo,
    TimeProvider timeProvider)
{
    public async Task<int> RevokeSessionsAsync(Ulid[] sessionIds)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await sessionRepo.BeginTransactionAsync();

        var sessionsToRevoke = await sessionRepo.GetActiveSessionsByIdsAsync(sessionIds);
        if (sessionsToRevoke.Count < sessionIds.Length)
            throw new BadRequestException("Some sessions are unavailable for revoke for certain reasons");

        await revocationValidator.EnsureRevocationAllowedAsync(sessionsToRevoke);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var session in sessionsToRevoke)
        {
            session.Status = SessionStatus.Close;
            session.DeactivatedAt = utcNow;
            session.DeactivatedBySessionId = currentUser.SessionId.GetIdAsUlid();
        }
        await sessionRepo.UpdateRangeAsync(sessionsToRevoke);

        var refreshTokens = await refreshTokenRepo.GetRefreshTokensBySessionIdsAsync(sessionIds);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.UsedAt = utcNow;
        }
        await refreshTokenRepo.UpdateRangeAsync(refreshTokens);

        await sessionManager.DeleteSessionsByIdsAsync(sessionsToRevoke.Select(s => s.Id.GetIdAsString()), currentUser.UserId);
        await transaction.CommitAsync();

        return sessionsToRevoke.Count;
    }
}
