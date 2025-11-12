namespace IdentityPrvd.Services.Security;

/// <summary>
/// Service for controlling user sessions
/// </summary>
public interface ISessionControlService
{
    /// <summary>
    /// Closes a specific session by its ID
    /// </summary>
    /// <param name="sessionId">The session ID to close</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CloseSessionByIdAsync(Ulid sessionId);

    /// <summary>
    /// Closes all active sessions for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CloseAllSessionsByUserIdAsync(Ulid userId);

    /// <summary>
    /// Closes all active sessions for a user except the specified session
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="sessionIdToKeep">The session ID to keep active</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CloseAllSessionsExceptAsync(Ulid userId, Ulid sessionIdToKeep);

    /// <summary>
    /// Closes all other active sessions for a user except the current session, if SingleSessionPerUser is enabled
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currentSessionId">The current session ID to keep active</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task CloseOtherSessionsIfRequiredAsync(Ulid userId, Ulid currentSessionId);
}

