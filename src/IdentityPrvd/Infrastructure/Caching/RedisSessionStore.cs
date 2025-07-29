using IdentityPrvd.Options;
using IdentityPrvd.Services.ServerSideSessions;
using LinqKit;
using Microsoft.Extensions.Logging;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;

namespace IdentityPrvd.Infrastructure.Caching;

public class RedisSessionStore(
    IRedisConnectionProvider provider,
    TokenOptions tokenOptions,
    ILogger<RedisSessionStore> logger) : ISessionStore
{
    private readonly IRedisCollection<SessionInfo> _sessions = provider.RedisCollection<SessionInfo>();
    private SessionInfo _currentSession;

    public async Task InitializeAsync()
    {
        await provider.Connection.CreateIndexAsync(typeof(SessionInfo));
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> AddSessionAsync(SessionInfo sessionInfo)
    {
        var sessionExists = await GetSessionAsync(sessionInfo.SessionId);
        if (sessionExists != null && sessionExists.UserId == sessionInfo.UserId)
        {
            logger.LogWarning("Session with Id:{SessionId} already exists for user {UserId}", sessionInfo.SessionId, sessionInfo.UserId);
            return (false, $"Session with Id:{sessionInfo.SessionId} already exists for user {sessionInfo.UserId}");
        }

        var key = await _sessions.InsertAsync(sessionInfo, TimeSpan.FromDays(tokenOptions.SessionLifeTimeInDays));
        return (true, $"Session with key:{key} added successfully.");
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> DeleteAllUserSessionsAsync(string userId)
    {
        var sessionsToDelete = await _sessions.Where(s => s.UserId == userId).ToListAsync();
        if (sessionsToDelete.Count == 0)
        {
            return (false, $"No sessions found for user {userId}.");
        }

        if (sessionsToDelete.Any(s => s.SessionId == _currentSession.SessionId))
            _currentSession = null;

        await _sessions.DeleteAsync(sessionsToDelete);

        return await Task.FromResult((true, "All user sessions deleted successfully."));
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> DeleteSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            logger.LogWarning("Session with Id:{SessionId} not found", sessionId);
            return (false, $"Session with Id:{sessionId} not found.");
        }
        if (_currentSession.SessionId == sessionId)
            _currentSession = null;
        await _sessions.DeleteAsync(session);
        return (true, "Session deleted successfully.");
    }

    public async Task<SessionInfo> GetSessionAsync(string sessionId)
    {
        return _currentSession ??= await _sessions.Where(s => s.SessionId == sessionId).FirstOrDefaultAsync();
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> UpdateSessionAsync(SessionInfo sessionInfo)
    {
        var existingSession = await GetSessionAsync(sessionInfo.SessionId);
        if (existingSession == null)
        {
            return (false, $"Session with Id:{sessionInfo.SessionId} not found.");
        }
        existingSession.LastAccessedAt = sessionInfo.LastAccessedAt;
        existingSession.SessionExpire = sessionInfo.SessionExpire;
        existingSession.Permissions = sessionInfo.Permissions;

        await _sessions.UpdateAsync(existingSession);
        return (true, "Session updated successfully.");
    }

    public async Task<IList<SessionInfo>> GetUserSessionsAsync(string userId)
    {
        return await _sessions.Where(s => s.UserId == userId).ToListAsync();
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> DeleteSessionsAsync(IList<SessionInfo> sessions)
    {
        await _sessions.DeleteAsync(sessions);

        if (sessions.Any(s => s.SessionId == _currentSession.SessionId))
            _currentSession = null;
        return (true, string.Empty);
    }

    public async Task<IList<SessionInfo>> GetSessionsByIdsAsync(IEnumerable<string> sessionIds)
    {
        var predicate = PredicateBuilder.New<SessionInfo>(false);

        foreach (var sid in sessionIds)
        {
            var local = sid;
            predicate = predicate.Or(x => x.SessionId == local);
        }
        var query = _sessions.Where(predicate).ToQueryString();

        return await _sessions.Where(predicate).ToListAsync();
    }
}
