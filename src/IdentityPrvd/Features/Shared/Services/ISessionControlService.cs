using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Options;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Features.Shared.Services;

/// <summary>
/// Service for controlling user sessions
/// </summary>
public interface ISessionControlService
{
    Task CloseSessionByIdAsync(Ulid sessionId);
    Task CloseActiveUserSessionsAsync(Ulid userId, Ulid[] sessionIdsToKeep = null);
    Task CloseOtherSessionsIfRequiredAsync(Ulid userId, Ulid currentSessionId);
}

public class SessionControlService(
    IdentityPrvdOptions identityOptions,
    ISessionStore sessionStore,
    IRefreshTokenStore refreshTokenStore,
    ISessionManager sessionManager,
    TimeProvider timeProvider) : ISessionControlService
{
    public async Task CloseActiveUserSessionsAsync(Ulid userId, Ulid[] sessionIdsToKeep = null)
    {
        var userSessions = await sessionStore.GetActiveSessionsByUserIdAsync(userId);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var userSession in userSessions)
        {
            if (sessionIdsToKeep == null || !sessionIdsToKeep.Contains(userSession.Id))
                await CloseSessionAsync(userSession.Id, utcNow);
        }
    }

    public async Task CloseOtherSessionsIfRequiredAsync(Ulid userId, Ulid currentSessionId)
    {
        if (identityOptions.SingleSessionPerUser)
        {
            await CloseActiveUserSessionsAsync(userId, [currentSessionId]);
        }
    }

    public async Task CloseSessionByIdAsync(Ulid sessionId)
    {
        await CloseSessionAsync(sessionId, timeProvider.GetUtcNow().UtcDateTime);
    }

    private async Task CloseSessionAsync(Ulid sessionId, DateTime utcNow)
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
