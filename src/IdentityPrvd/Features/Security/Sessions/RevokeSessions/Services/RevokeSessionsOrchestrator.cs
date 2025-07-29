using IdentityPrvd.Common.Constants;
using IdentityPrvd.Common.Exceptions;
using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Contexts;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Data.Transactions;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Security.Sessions.RevokeSessions.Services;

public class RevokeSessionsOrchestrator(
    IUserContext userContext,
    ISessionManager sessionManager,
    SessionRevocationValidator revocationValidator,
    ISessionStore sessionStore,
    IRefreshTokenStore refreshTokenStore,
    ITransactionManager transactionManager,
    TimeProvider timeProvider)
{
    public async Task<int> RevokeSessionsAsync(Ulid[] sessionIds)
    {
        var currentUser = userContext.AssumeAuthenticated<BasicAuthenticatedUser>();
        currentUser.EnsureUserHasPermissionsOrRoles(
            IdentityClaims.Types.Identity, IdentityClaims.Values.All,
            [DefaultsRoles.SuperAdmin, DefaultsRoles.Admin]);

        await using var transaction = await transactionManager.BeginTransactionAsync();

        var sessionsToRevoke = await sessionStore.GetActiveSessionsByIdsAsync(sessionIds);
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
        await sessionStore.UpdateRangeAsync(sessionsToRevoke);

        var refreshTokens = await refreshTokenStore.GetRefreshTokensBySessionIdsAsync(sessionIds);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.UsedAt = utcNow;
        }
        await refreshTokenStore.UpdateRangeAsync(refreshTokens);

        await sessionManager.DeleteSessionsByIdsAsync(sessionsToRevoke.Select(s => s.Id.GetIdAsString()), currentUser.UserId);
        await transaction.CommitAsync();

        return sessionsToRevoke.Count;
    }
}
