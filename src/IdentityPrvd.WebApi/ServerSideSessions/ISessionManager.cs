using IdentityPrvd.WebApi.Exceptions;
using IdentityPrvd.WebApi.ServerSideSessions.Stores;

namespace IdentityPrvd.WebApi.ServerSideSessions;

public interface ISessionManager
{
    Task<bool> IsActiveSessionAsync(string userId, string sessionId);
    Task<Dictionary<string, List<string>>> GetSessionPermissionsAsync(string sessionId);
    Task<IList<SessionInfo>> GetUserSessionsAsync(string userId);
    Task AddNewSessionAsync(SessionInfo sessionInfo);
    Task UpdateSessionAsync(SessionInfo sessionInfo);
    Task DeleteSessionAsync(string userId, string sessionId);
    Task DeleteSessionsByIdsAsync(IEnumerable<string> sessionIds, string userId);
    Task DeleteAllUserSessionsAsync(string userId);
    Task MarkSessionLastActivityAsync(string userId, string sessionId);
}

public class SessionManager(
    ISessionStore sessionStore,
    ILogger<SessionManager> logger,
    TimeProvider timeProvider) : ISessionManager
{
    public async Task AddNewSessionAsync(SessionInfo sessionInfo)
    {
        var (IsSuccess, ErrorMessage) = await sessionStore.AddSessionAsync(sessionInfo);

        if (!IsSuccess)
        {
            logger.LogError("Failed to add session for user {UserId}: {ErrorMessage}", sessionInfo.UserId, ErrorMessage);
            throw new InternalServerError($"Failed to add session: {ErrorMessage}");
        }
    }

    public async Task<bool> IsActiveSessionAsync(string userId, string sessionId)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var session = await sessionStore.GetSessionAsync(sessionId);
        if (session == null)
        {
            logger.LogWarning("Session not found for user {UserId} with session ID {SessionId}", userId, sessionId);
            return false;
        }
        if (session.UserId != userId)
        {
            logger.LogWarning("Session {SessionId} does not belong to user {UserId}", sessionId, userId);
            return false;
        }
        if (session.SessionExpire < utcNow)
        {
            logger.LogWarning("Session {SessionId} for user {UserId} has expired", sessionId, userId);
            return false;
        }
        return true;
    }

    public async Task DeleteAllUserSessionsAsync(string userId)
    {
        var (IsSuccess, ErrorMessage) = await sessionStore.DeleteAllUserSessionsAsync(userId);
        if (!IsSuccess)
        {
            logger.LogError("Failed to delete all sessions for user {UserId}: {ErrorMessage}", userId, ErrorMessage);
            throw new InternalServerError($"Failed to delete all sessions: {ErrorMessage}");
        }
    }

    public async Task DeleteSessionAsync(string userId, string sessionId)
    {
        var (IsSuccess, ErrorMessage) = await sessionStore.DeleteSessionAsync(sessionId);
        if (!IsSuccess)
        {
            logger.LogError("Failed to delete session {SessionId} for user {UserId}: {ErrorMessage}", sessionId, userId, ErrorMessage);
            throw new InternalServerError($"Failed to delete session: {ErrorMessage}");
        }
    }

    public async Task UpdateSessionAsync(SessionInfo sessionInfo)
    {
        var (IsSuccess, ErrorMessage) = await sessionStore.UpdateSessionAsync(sessionInfo);
        if (!IsSuccess)
        {
            logger.LogError("Failed to update session {SessionId} for user {UserId}: {ErrorMessage}", sessionInfo.SessionId, sessionInfo.UserId, ErrorMessage);
            throw new InternalServerError($"Failed to update session: {ErrorMessage}");
        }
    }

    public async Task MarkSessionLastActivityAsync(string userId, string sessionId)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var session = await sessionStore.GetSessionAsync(sessionId);
        if (session == null)
        {
            logger.LogWarning("Session not found for user {UserId} with session ID {SessionId}", userId, sessionId);
            return;
        }

        session.LastAccessedAt = utcNow;
        await sessionStore.UpdateSessionAsync(session);
    }

    public async Task<Dictionary<string, List<string>>> GetSessionPermissionsAsync(string sessionId)
    {
        var session = await sessionStore.GetSessionAsync(sessionId);

        if (session == null)
        {
            return [];
        }

        return session.Permissions;
    }

    public async Task<IList<SessionInfo>> GetUserSessionsAsync(string userId)
    {
        return await sessionStore.GetUserSessionsAsync(userId);
    }

    public async Task DeleteSessionsByIdsAsync(IEnumerable<string> sessionIds, string userId)
    {
        var sessions = await sessionStore.GetSessionsByIdsAsync(sessionIds);

        if (sessions.Any(s => s.UserId != userId))
            throw new BadRequestException("Some sessions does not belong to current user");

        await sessionStore.DeleteSessionsAsync(sessions);
    }
}
