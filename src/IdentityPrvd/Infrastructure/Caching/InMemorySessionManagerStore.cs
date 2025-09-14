using IdentityPrvd.Common.Extensions;
using IdentityPrvd.Data.Queries;
using IdentityPrvd.Services.Security;
using IdentityPrvd.Services.ServerSideSessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityPrvd.Infrastructure.Caching;

public class InMemorySessionManagerStore(IServiceProvider serviceProvider) : ISessionManagerStore
{
    private List<SessionInfo> _activeSessions = [];

    public async Task<SessionInfo> GetSessionAsync(string sessionId, bool currentSession = false)
    {
        return await Task.FromResult(_activeSessions.FirstOrDefault(s => s.SessionId == sessionId)!);
    }

    public async Task<(bool, string)> AddSessionAsync(SessionInfo sessionInfo)
    {
        var sessionExists = await GetSessionAsync(sessionInfo.SessionId);
        if (sessionExists != null && sessionExists.UserId == sessionInfo.UserId)
        {
            return (false, $"Session with Id:{sessionInfo.SessionId} already exists for user {sessionInfo.UserId}");
        }

        _activeSessions.Add(sessionInfo);
        return (true, "Session added successfully.");
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> DeleteSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null)
        {
            return (false, $"Session with Id:{sessionId} not found.");
        }
        _activeSessions.Remove(session);
        return (true, "Session deleted successfully.");
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> DeleteAllUserSessionsAsync(string userId)
    {
        var sessionsToDelete = _activeSessions.Where(s => s.UserId == userId);
        if (!sessionsToDelete.Any())
        {
            return (false, $"No sessions found for user {userId}.");
        }

        _activeSessions.RemoveAll(s => s.UserId == userId);

        return await Task.FromResult((true, "All user sessions deleted successfully."));
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
        return (true, "Session updated successfully.");
    }

    public async Task InitializeAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var sessionsQuery = scope.ServiceProvider.GetRequiredService<ISessionsQuery>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var activeSessions = await sessionsQuery.GetAllActiveSessionsAsync();

        foreach (var session in activeSessions)
        {
            _activeSessions.Add(new SessionInfo
            {
                SessionId = session.Id.GetIdAsString(),
                UserId = session.UserId.GetIdAsString(),
                CreatedAt = session.CreatedAt,
                Permissions = await tokenService.GetUserPermissionsAsync(session.UserId, session.App.Id.GetIdAsString()),
                SessionExpire = session.ExpireAt,
            });
        }
    }

    public async Task<IList<SessionInfo>> GetUserSessionsAsync(string userId)
    {
        return await Task.FromResult(_activeSessions.Where(s => s.UserId == userId).ToList());
    }

    public async Task<(bool IsSuccess, string ErrorMessage)> DeleteSessionsAsync(IList<SessionInfo> sessions)
    {
        foreach (var session in sessions)
        {
            _activeSessions.Remove(session);
        }

        return await Task.FromResult((true, string.Empty));
    }

    public async Task<IList<SessionInfo>> GetSessionsByIdsAsync(IEnumerable<string> sessionIds)
    {
        return await Task.FromResult(_activeSessions.Where(s => sessionIds.Contains(s.SessionId)).ToList());
    }
}
