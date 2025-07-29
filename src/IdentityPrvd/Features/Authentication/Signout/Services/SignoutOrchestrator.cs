using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Authentication.Signout.Services;

public class SignoutOrchestrator(
    IUserContext userContext,
    ISessionStore sessionStore,
    IRefreshTokenStore refreshTokenStore,
    ITransactionManager transactionManager,
    TimeProvider timeProvider,
    ISessionManager sessionManager)
{
    public async Task SignoutAsync(bool everywhere)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

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
        var userSessions = await sessionStore.GetActiveSessionsByUserIdAsync(currentUser.UserId.GetIdAsUlid());

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
        var session = await sessionStore.GetAsync(sessionId);
        session.Status = SessionStatus.Close;
        session.DeactivatedAt = utcNow;
        session.DeactivatedBySessionId = sessionId;
        await sessionStore.UpdateAsync(session);
        var refreshTokens = await refreshTokenStore.GetRefreshTokensBySessionIdAsync(sessionId);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.UsedAt = utcNow;
        }
        await refreshTokenStore.UpdateRangeAsync(refreshTokens);
        await sessionManager.DeleteSessionAsync(session.UserId.GetIdAsString(), sessionId.GetIdAsString());
    }
}
