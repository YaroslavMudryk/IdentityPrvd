using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Stores;
using IdentityPrvd.Domain.Enums;
using IdentityPrvd.Options;
using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Services.Security;

/// <summary>
/// Service for controlling user sessions
/// </summary>
public class SessionControlService(
    IdentityPrvdOptions identityOptions,
    ISessionStore sessionStore,
    IRefreshTokenStore refreshTokenStore,
    ISessionManager sessionManager,
    TimeProvider timeProvider) : ISessionControlService
{
    public async Task CloseSessionByIdAsync(Ulid sessionId)
    {
        var session = await sessionStore.GetAsync(sessionId);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        
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

    public async Task CloseAllSessionsByUserIdAsync(Ulid userId)
    {
        var userSessions = await sessionStore.GetActiveSessionsByUserIdAsync(userId);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        
        foreach (var userSession in userSessions)
        {
            await CloseSessionByIdInternalAsync(userSession, utcNow);
        }
    }

    public async Task CloseAllSessionsExceptAsync(Ulid userId, Ulid sessionIdToKeep)
    {
        var userSessions = await sessionStore.GetActiveSessionsByUserIdAsync(userId);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        
        foreach (var userSession in userSessions)
        {
            if (userSession.Id != sessionIdToKeep)
            {
                await CloseSessionByIdInternalAsync(userSession, utcNow);
            }
        }
    }

    public async Task CloseOtherSessionsIfRequiredAsync(Ulid userId, Ulid currentSessionId)
    {
        if (identityOptions.SingleSessionPerUser)
        {
            await CloseAllSessionsExceptAsync(userId, currentSessionId);
        }
    }

    private async Task CloseSessionByIdInternalAsync(Domain.Entities.IdentitySession session, DateTime utcNow)
    {
        session.Status = SessionStatus.Close;
        session.DeactivatedAt = utcNow;
        session.DeactivatedBySessionId = session.Id;
        await sessionStore.UpdateAsync(session);
        
        var refreshTokens = await refreshTokenStore.GetRefreshTokensBySessionIdAsync(session.Id);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.UsedAt = utcNow;
        }
        await refreshTokenStore.UpdateRangeAsync(refreshTokens);
        
        await sessionManager.DeleteSessionAsync(session.UserId.GetIdAsString(), session.Id.GetIdAsString());
    }
}

