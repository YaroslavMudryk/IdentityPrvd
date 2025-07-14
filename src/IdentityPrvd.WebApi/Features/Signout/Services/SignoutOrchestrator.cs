using IdentityPrvd.WebApi.Db.Entities.Enums;
using IdentityPrvd.WebApi.Extensions;
using IdentityPrvd.WebApi.Features.Signout.DataAccess;
using IdentityPrvd.WebApi.Helpers;
using IdentityPrvd.WebApi.ServerSideSessions;
using IdentityPrvd.WebApi.UserContext;

namespace IdentityPrvd.WebApi.Features.Signout.Services;

public class SignoutOrchestrator(
    IUserContext userContext,
    SessionRepo sessionRepo,
    RefreshTokenRepo refreshTokenRepo,
    TimeProvider timeProvider,
    ISessionManager sessionManager)
{
    public async Task SignoutAsync(bool everywhere)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await sessionRepo.BeginTransactionAsync();

        if (!everywhere)
        {
            await HandleSignoutAsync(currentUser);
        }
        else
        {
            await HandleSignoutEverywhereAsync(currentUser);
        }

        await transaction.CommitAsync();
    }

    public async Task HandleSignoutEverywhereAsync(BasicAuthenticatedUser currentUser)
    {
        var userSessions = await sessionRepo.GetActiveSessionsByUserIdAsync(currentUser.UserId.GetIdAsUlid());

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var userSession in userSessions)
        {
            await CloseSessionByIdAsync(userSession.Id, utcNow);
        }
    }

    public async Task HandleSignoutAsync(BasicAuthenticatedUser currentUser)
    {
        var sessionId = currentUser.SessionId.GetIdAsUlid();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        await CloseSessionByIdAsync(sessionId, utcNow);
    }

    public async Task CloseSessionByIdAsync(Ulid sessionId, DateTime utcNow)
    {
        var session = await sessionRepo.GetAsync(sessionId);
        session.Status = SessionStatus.Close;
        session.DeactivatedAt = utcNow;
        session.DeactivatedBySessionId = sessionId;
        await sessionRepo.UpdateAsync(session);
        var refreshTokens = await refreshTokenRepo.GetRefreshTokensBySessionIdAsync(sessionId);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.UsedAt = utcNow;
        }
        await refreshTokenRepo.UpdateRangeAsync(refreshTokens);
        await sessionManager.DeleteSessionAsync(session.UserId.GetIdAsString(), sessionId.GetIdAsString());
    }
}
