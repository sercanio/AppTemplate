using AppTemplate.Application.Services.Statistics;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text.Json;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Services.Statistics;

[Trait("Category", "Unit")]
public class ActiveSessionServiceUnitTests
{
  private readonly Mock<IDistributedCache> _cacheMock;
  private readonly ActiveSessionService _service;

  public ActiveSessionServiceUnitTests()
  {
    _cacheMock = new Mock<IDistributedCache>();
    _service = new ActiveSessionService(_cacheMock.Object);
  }

  [Fact]
  public async Task RecordUserActivityAsync_AddsOrUpdatesSession()
  {
    // Arrange
    var userId = "user1";
    var sessions = new Dictionary<string, DateTime>();
    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync((string)null);

    _cacheMock.Setup(c => c.SetStringAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DistributedCacheEntryOptions>(),
        default))
        .Returns(Task.CompletedTask)
        .Verifiable();

    // Act
    await _service.RecordUserActivityAsync(userId);

    // Assert
    _cacheMock.Verify(c => c.SetStringAsync(
        "activesessions",
        It.Is<string>(s => s.Contains(userId)),
        It.IsAny<DistributedCacheEntryOptions>(),
        default), Times.Once);
  }

  [Fact]
  public async Task RecordUserActivityAsync_UpdatesExistingSession()
  {
    // Arrange
    var userId = "user1";
    var oldTime = DateTime.UtcNow.AddMinutes(-10);
    var sessions = new Dictionary<string, DateTime> { { userId, oldTime } };
    var serialized = JsonSerializer.Serialize(sessions);

    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync(serialized);

    _cacheMock.Setup(c => c.SetStringAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DistributedCacheEntryOptions>(),
        default))
        .Returns(Task.CompletedTask)
        .Verifiable();

    // Act
    await _service.RecordUserActivityAsync(userId);

    // Assert
    _cacheMock.Verify(c => c.SetStringAsync(
        "activesessions",
        It.Is<string>(s => s.Contains(userId)),
        It.IsAny<DistributedCacheEntryOptions>(),
        default), Times.Once);
  }

  [Fact]
  public async Task RemoveUserSessionAsync_RemovesSessionIfExists()
  {
    // Arrange
    var userId = "user1";
    var sessions = new Dictionary<string, DateTime> { { userId, DateTime.UtcNow } };
    var serialized = JsonSerializer.Serialize(sessions);

    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync(serialized);

    _cacheMock.Setup(c => c.SetStringAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DistributedCacheEntryOptions>(),
        default))
        .Returns(Task.CompletedTask)
        .Verifiable();

    // Act
    await _service.RemoveUserSessionAsync(userId);

    // Assert
    _cacheMock.Verify(c => c.SetStringAsync(
        "activesessions",
        It.Is<string>(s => !s.Contains(userId)),
        It.IsAny<DistributedCacheEntryOptions>(),
        default), Times.Once);
  }

  [Fact]
  public async Task RemoveUserSessionAsync_DoesNothingIfSessionNotExists()
  {
    // Arrange
    var userId = "user1";
    var sessions = new Dictionary<string, DateTime> { { "user2", DateTime.UtcNow } };
    var serialized = JsonSerializer.Serialize(sessions);

    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync(serialized);

    // Act
    await _service.RemoveUserSessionAsync(userId);

    // Assert
    _cacheMock.Verify(c => c.SetStringAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DistributedCacheEntryOptions>(),
        default), Times.Never);
  }

  [Fact]
  public async Task GetActiveSessionsCountAsync_ReturnsCorrectCount()
  {
    // Arrange
    var now = DateTime.UtcNow;
    var sessions = new Dictionary<string, DateTime>
        {
            { "user1", now },
            { "user2", now.AddMinutes(-10) },
            { "user3", now.AddMinutes(-40) } // expired
        };
    var serialized = JsonSerializer.Serialize(sessions);

    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync(serialized);

    // Act
    var count = await _service.GetActiveSessionsCountAsync();

    // Assert
    Assert.Equal(2, count);
  }

  [Fact]
  public async Task GetActiveSessionsCountAsync_ReturnsZeroIfNoSessions()
  {
    // Arrange
    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync((string)null);

    // Act
    var count = await _service.GetActiveSessionsCountAsync();

    // Assert
    Assert.Equal(0, count);
  }

  [Fact]
  public async Task GetActiveSessionsAsync_ReturnsOnlyActiveSessions()
  {
    // Arrange
    var now = DateTime.UtcNow;
    var sessions = new Dictionary<string, DateTime>
        {
            { "user1", now },
            { "user2", now.AddMinutes(-10) },
            { "user3", now.AddMinutes(-40) } // expired
        };
    var serialized = JsonSerializer.Serialize(sessions);

    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync(serialized);

    // Act
    var activeSessions = await _service.GetActiveSessionsAsync();

    // Assert
    Assert.Equal(2, activeSessions.Count);
    Assert.Contains("user1", activeSessions.Keys);
    Assert.Contains("user2", activeSessions.Keys);
    Assert.DoesNotContain("user3", activeSessions.Keys);
  }

  [Fact]
  public async Task GetActiveSessionsAsync_ReturnsEmptyIfNoSessions()
  {
    // Arrange
    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync((string)null);

    // Act
    var activeSessions = await _service.GetActiveSessionsAsync();

    // Assert
    Assert.Empty(activeSessions);
  }

  [Fact]
  public async Task GetSessionsFromCacheAsync_ReturnsEmptyIfCacheIsEmpty()
  {
    // Arrange
    _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
        .ReturnsAsync((string)null);

    // Use reflection to call private method for coverage
    var method = typeof(ActiveSessionService).GetMethod("GetSessionsFromCacheAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    // Act
    var task = (Task<Dictionary<string, DateTime>>)method.Invoke(_service, null);
    var result = await task;

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task SaveSessionsToCacheAsync_RemovesExpiredSessions()
  {
    // Arrange
    var now = DateTime.UtcNow;
    var sessions = new Dictionary<string, DateTime>
        {
            { "user1", now },
            { "user2", now.AddMinutes(-40) } // expired
        };

    string? serializedResult = null;
    _cacheMock.Setup(c => c.SetStringAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DistributedCacheEntryOptions>(),
        default))
        .Callback<string, string, DistributedCacheEntryOptions, System.Threading.CancellationToken>((key, value, options, token) =>
        {
          serializedResult = value;
        })
        .Returns(Task.CompletedTask);

    // Use reflection to call private method for coverage
    var method = typeof(ActiveSessionService).GetMethod("SaveSessionsToCacheAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    // Act
    var task = (Task)method.Invoke(_service, new object[] { sessions });
    await task;

    // Assert
    Assert.NotNull(serializedResult);
    var deserialized = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(serializedResult);
    Assert.Single(deserialized);
    Assert.Contains("user1", deserialized.Keys);
    Assert.DoesNotContain("user2", deserialized.Keys);
  }
}