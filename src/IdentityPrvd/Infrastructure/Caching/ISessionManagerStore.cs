using IdentityPrvd.Services.ServerSideSessions;

namespace IdentityPrvd.Infrastructure.Caching;

public interface ISessionManagerStore
{
    Task InitializeAsync();
    Task<SessionInfo> GetSessionAsync(string sessionId, bool currentSession = false);
    Task<IList<SessionInfo>> GetUserSessionsAsync(string userId);
    Task<(bool IsSuccess, string ErrorMessage)> AddSessionAsync(SessionInfo sessionInfo);
    Task<(bool IsSuccess, string ErrorMessage)> UpdateSessionAsync(SessionInfo sessionInfo);
    Task<(bool IsSuccess, string ErrorMessage)> DeleteSessionAsync(string sessionId);
    Task<(bool IsSuccess, string ErrorMessage)> DeleteAllUserSessionsAsync(string userId);
    Task<(bool IsSuccess, string ErrorMessage)> DeleteSessionsAsync(IList<SessionInfo> sessions);
    Task<IList<SessionInfo>> GetSessionsByIdsAsync(IEnumerable<string> sessionIds);
}
