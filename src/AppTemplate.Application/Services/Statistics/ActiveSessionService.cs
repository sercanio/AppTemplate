using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace AppTemplate.Application.Services.Statistics;

public class ActiveSessionService : IActiveSessionService
{
  private readonly IDistributedCache _cache;
  private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);
  private const string ActiveSessionsKey = "activesessions";

  public ActiveSessionService(IDistributedCache cache)
  {
    _cache = cache;
  }

  public async Task RecordUserActivityAsync(string userId)
  {
    var sessions = await GetSessionsFromCacheAsync();
    sessions[userId] = DateTime.UtcNow;
    await SaveSessionsToCacheAsync(sessions);
  }

  public async Task RemoveUserSessionAsync(string userId)
  {
    var sessions = await GetSessionsFromCacheAsync();
    if (sessions.ContainsKey(userId))
    {
      sessions.Remove(userId);
      await SaveSessionsToCacheAsync(sessions);
    }
  }

  public async Task<int> GetActiveSessionsCountAsync()
  {
    var sessions = await GetSessionsFromCacheAsync();
    var currentTime = DateTime.UtcNow;

    // Filter out expired sessions
    return sessions.Count(s => currentTime.Subtract(s.Value) <= _sessionTimeout);
  }

  public async Task<Dictionary<string, DateTime>> GetActiveSessionsAsync()
  {
    var sessions = await GetSessionsFromCacheAsync();
    var currentTime = DateTime.UtcNow;

    // Filter out expired sessions
    return sessions
        .Where(s => currentTime.Subtract(s.Value) <= _sessionTimeout)
        .ToDictionary(k => k.Key, v => v.Value);
  }

  private async Task<Dictionary<string, DateTime>> GetSessionsFromCacheAsync()
  {
    var cachedSessions = await _cache.GetStringAsync(ActiveSessionsKey);

    if (string.IsNullOrEmpty(cachedSessions))
    {
      return new Dictionary<string, DateTime>();
    }

    return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(cachedSessions)
           ?? new Dictionary<string, DateTime>();
  }

  private async Task SaveSessionsToCacheAsync(Dictionary<string, DateTime> sessions)
  {
    var currentTime = DateTime.UtcNow;

    // Clean up expired sessions before saving
    var activeSessions = sessions
        .Where(s => currentTime.Subtract(s.Value) <= _sessionTimeout)
        .ToDictionary(k => k.Key, v => v.Value);

    var serializedSessions = JsonSerializer.Serialize(activeSessions);

    await _cache.SetStringAsync(
        ActiveSessionsKey,
        serializedSessions,
        new DistributedCacheEntryOptions
        {
          AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
        });
  }
}