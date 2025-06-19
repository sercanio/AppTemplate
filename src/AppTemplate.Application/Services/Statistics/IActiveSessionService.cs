namespace AppTemplate.Application.Services.Statistics;

/// <summary>
/// Service for tracking and querying active user sessions
/// </summary>
public interface IActiveSessionService
{
    /// <summary>
    /// Records user activity to mark session as active
    /// </summary>
    /// <param name="userId">The user identifier</param>
    Task RecordUserActivityAsync(string userId);

    /// <summary>
    /// Removes user session when user signs out
    /// </summary>
    /// <param name="userId">The user identifier</param>
    Task RemoveUserSessionAsync(string userId);

    /// <summary>
    /// Gets the count of currently active sessions
    /// </summary>
    Task<int> GetActiveSessionsCountAsync();

    /// <summary>
    /// Gets all active sessions with their last activity time
    /// </summary>
    Task<Dictionary<string, DateTime>> GetActiveSessionsAsync();
}