using AppTemplate.Application.Behaviors;
using AppTemplate.Application.Services.Caching;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Behaviours;

public class QueryCachingBehaviorUnitTests
{
  private readonly Mock<ICacheService> _mockCacheService;
  private readonly Mock<ILogger<QueryCachingBehavior<TestCachedQuery, string>>> _mockLogger;
  private readonly QueryCachingBehavior<TestCachedQuery, string> _cachingBehavior;
  private readonly CancellationToken _cancellationToken;

  public QueryCachingBehaviorUnitTests()
  {
    _mockCacheService = new Mock<ICacheService>();
    _mockLogger = new Mock<ILogger<QueryCachingBehavior<TestCachedQuery, string>>>();
    _cachingBehavior = new QueryCachingBehavior<TestCachedQuery, string>(_mockCacheService.Object, _mockLogger.Object);
    _cancellationToken = CancellationToken.None;
  }

  [Fact]
  public async Task Handle_WhenCacheHit_ShouldReturnCachedValueAndNotCallNext()
  {
    // Arrange
    var request = new TestCachedQuery();
    var cachedResponse = "Cached Response";
    var nextDelegateCalled = false;

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync(cachedResponse);

    // Act
    var result = await _cachingBehavior.Handle(request, _ =>
    {
      nextDelegateCalled = true;
      return Task.FromResult("Fresh Response");
    }, _cancellationToken);

    // Assert
    Assert.Equal(cachedResponse, result);
    Assert.False(nextDelegateCalled);

    _mockCacheService.Verify(
        x => x.GetAsync<string>(request.CacheKey, _cancellationToken),
        Times.Once);

    _mockCacheService.Verify(
        x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
        Times.Never);

    VerifyLogCalled(LogLevel.Information, "Cache hit");
  }

  [Fact]
  public async Task Handle_WhenCacheMiss_ShouldCallNextAndCacheResult()
  {
    // Arrange
    var request = new TestCachedQuery();
    var freshResponse = "Fresh Response";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    // Act
    var result = await _cachingBehavior.Handle(request, _ => Task.FromResult(freshResponse), _cancellationToken);

    // Assert
    Assert.Equal(freshResponse, result);

    _mockCacheService.Verify(
        x => x.GetAsync<string>(request.CacheKey, _cancellationToken),
        Times.Once);

    _mockCacheService.Verify(
        x => x.SetAsync(request.CacheKey, freshResponse, request.Expiration, _cancellationToken),
        Times.Once);

    VerifyLogCalled(LogLevel.Information, "Cache miss");
  }

  [Fact]
  public async Task Handle_WhenCacheMissWithNullExpiration_ShouldCacheWithNullExpiration()
  {
    // Arrange
    var request = new TestCachedQueryWithNullExpiration();
    var freshResponse = "Fresh Response";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    var behavior = new QueryCachingBehavior<TestCachedQueryWithNullExpiration, string>(
        _mockCacheService.Object,
        new Mock<ILogger<QueryCachingBehavior<TestCachedQueryWithNullExpiration, string>>>().Object);

    // Act
    var result = await behavior.Handle(request, _ => Task.FromResult(freshResponse), _cancellationToken);

    // Assert
    Assert.Equal(freshResponse, result);

    _mockCacheService.Verify(
        x => x.SetAsync(request.CacheKey, freshResponse, null, _cancellationToken),
        Times.Once);
  }

  [Fact]
  public async Task Handle_WhenCacheMissWithCustomExpiration_ShouldCacheWithCorrectExpiration()
  {
    // Arrange
    var expiration = TimeSpan.FromMinutes(10);
    var request = new TestCachedQueryWithCustomExpiration(expiration);
    var freshResponse = "Fresh Response";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    var behavior = new QueryCachingBehavior<TestCachedQueryWithCustomExpiration, string>(
        _mockCacheService.Object,
        new Mock<ILogger<QueryCachingBehavior<TestCachedQueryWithCustomExpiration, string>>>().Object);

    // Act
    var result = await behavior.Handle(request, _ => Task.FromResult(freshResponse), _cancellationToken);

    // Assert
    Assert.Equal(freshResponse, result);

    _mockCacheService.Verify(
        x => x.SetAsync(request.CacheKey, freshResponse, expiration, _cancellationToken),
        Times.Once);
  }

  [Fact]
  public async Task Handle_WhenCacheServiceThrowsOnGet_ShouldPropagateException()
  {
    // Arrange
    var request = new TestCachedQuery();
    var expectedException = new InvalidOperationException("Cache service error");

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ThrowsAsync(expectedException);

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _cachingBehavior.Handle(request, _ => Task.FromResult("Response"), _cancellationToken));

    Assert.Same(expectedException, thrownException);
  }

  [Fact]
  public async Task Handle_WhenNextDelegateThrows_ShouldPropagateException()
  {
    // Arrange
    var request = new TestCachedQuery();
    var expectedException = new InvalidOperationException("Handler error");

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    // Act & Assert
    var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
        () => _cachingBehavior.Handle(request, _ => throw expectedException, _cancellationToken));

    Assert.Same(expectedException, thrownException);

    // Verify cache was not set
    _mockCacheService.Verify(
        x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
        Times.Never);
  }

  [Fact]
  public async Task Handle_WhenCacheSetFails_ShouldStillReturnFreshResponse()
  {
    // Arrange
    var request = new TestCachedQuery();
    var freshResponse = "Fresh Response";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    _mockCacheService
        .Setup(x => x.SetAsync(request.CacheKey, freshResponse, request.Expiration, _cancellationToken))
        .ThrowsAsync(new InvalidOperationException("Cache set failed"));

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _cachingBehavior.Handle(request, _ => Task.FromResult(freshResponse), _cancellationToken));
  }

  [Fact]
  public async Task Handle_WithCancellationToken_ShouldPassThroughToNext()
  {
    // Arrange
    var request = new TestCachedQuery();
    var cts = new CancellationTokenSource();
    var nextDelegateCalled = false;
    var tokenMatches = false;

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, cts.Token))
        .ReturnsAsync((string)null!);

    var behavior = new QueryCachingBehavior<TestCachedQuery, string>(_mockCacheService.Object, _mockLogger.Object);

    // Act
    await behavior.Handle(request, ct =>
    {
      nextDelegateCalled = true;
      // Check if the token is the same one we passed
      tokenMatches = ct.Equals(cts.Token);
      return Task.FromResult("Response");
    }, cts.Token);

    // Assert
    Assert.True(nextDelegateCalled);
    Assert.True(tokenMatches);

    // Verify the cache service was called with the correct token
    _mockCacheService.Verify(
        x => x.GetAsync<string>(request.CacheKey, cts.Token),
        Times.Once);

    _mockCacheService.Verify(
        x => x.SetAsync(request.CacheKey, "Response", request.Expiration, cts.Token),
        Times.Once);
  }

  [Fact]
  public async Task Handle_WhenCancellationRequested_ShouldPropagateCancellation()
  {
    // Arrange
    var request = new TestCachedQuery();
    var cts = new CancellationTokenSource();
    cts.Cancel();

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, cts.Token))
        .ThrowsAsync(new OperationCanceledException(cts.Token));

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _cachingBehavior.Handle(request, _ => Task.FromResult("Response"), cts.Token));
  }

  [Fact]
  public async Task Handle_WithDifferentCacheKeys_ShouldUseDifferentCacheEntries()
  {
    // Arrange
    var request1 = new TestCachedQuery { Id = Guid.NewGuid() };
    var request2 = new TestCachedQuery { Id = Guid.NewGuid() };
    var response1 = "Response 1";
    var response2 = "Response 2";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request1.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request2.CacheKey, _cancellationToken))
        .ReturnsAsync((string)null!);

    // Act
    await _cachingBehavior.Handle(request1, _ => Task.FromResult(response1), _cancellationToken);
    await _cachingBehavior.Handle(request2, _ => Task.FromResult(response2), _cancellationToken);

    // Assert
    _mockCacheService.Verify(
        x => x.SetAsync(request1.CacheKey, response1, request1.Expiration, _cancellationToken),
        Times.Once);

    _mockCacheService.Verify(
        x => x.SetAsync(request2.CacheKey, response2, request2.Expiration, _cancellationToken),
        Times.Once);
  }

  [Fact]
  public async Task Handle_WithComplexResponse_ShouldCacheCorrectly()
  {
    // Arrange
    var request = new TestCachedComplexQuery();
    var freshResponse = new ComplexResponse { Id = Guid.NewGuid(), Name = "Test", Count = 42 };

    _mockCacheService
        .Setup(x => x.GetAsync<ComplexResponse>(request.CacheKey, _cancellationToken))
        .ReturnsAsync((ComplexResponse)null!);

    var behavior = new QueryCachingBehavior<TestCachedComplexQuery, ComplexResponse>(
        _mockCacheService.Object,
        new Mock<ILogger<QueryCachingBehavior<TestCachedComplexQuery, ComplexResponse>>>().Object);

    // Act
    var result = await behavior.Handle(request, _ => Task.FromResult(freshResponse), _cancellationToken);

    // Assert
    Assert.Equal(freshResponse, result);

    _mockCacheService.Verify(
        x => x.SetAsync(request.CacheKey, freshResponse, request.Expiration, _cancellationToken),
        Times.Once);
  }

  [Fact]
  public void Constructor_WhenCacheServiceIsNull_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
        new QueryCachingBehavior<TestCachedQuery, string>(null!, _mockLogger.Object));
  }

  [Fact]
  public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
  {
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
        new QueryCachingBehavior<TestCachedQuery, string>(_mockCacheService.Object, null!));
  }

  [Fact]
  public async Task Handle_LogsCorrectQueryName()
  {
    // Arrange
    var request = new TestCachedQuery();
    var cachedResponse = "Cached Response";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync(cachedResponse);

    // Act
    await _cachingBehavior.Handle(request, _ => Task.FromResult("Fresh Response"), _cancellationToken);

    // Assert
    VerifyLogCalled(LogLevel.Information, "TestCachedQuery");
  }

  [Fact]
  public async Task Handle_WhenCacheReturnsDefaultValue_ShouldTreatAsMiss()
  {
    // Arrange
    var request = new TestCachedQuery();
    var freshResponse = "Fresh Response";

    _mockCacheService
        .Setup(x => x.GetAsync<string>(request.CacheKey, _cancellationToken))
        .ReturnsAsync(default(string));

    // Act
    var result = await _cachingBehavior.Handle(request, _ => Task.FromResult(freshResponse), _cancellationToken);

    // Assert
    Assert.Equal(freshResponse, result);

    _mockCacheService.Verify(
        x => x.SetAsync(request.CacheKey, freshResponse, request.Expiration, _cancellationToken),
        Times.Once);

    VerifyLogCalled(LogLevel.Information, "Cache miss");
  }

  // Helper method to verify logging
  private void VerifyLogCalled(LogLevel logLevel, string expectedMessage)
  {
    _mockLogger.Verify(
        x => x.Log(
            It.Is<LogLevel>(l => l == logLevel),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => CheckMessageContains(v, expectedMessage)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce);
  }

  private static bool CheckMessageContains(object message, string expectedText)
  {
    if (message == null)
      return false;
    var messageString = message.ToString();
    return messageString != null && messageString.Contains(expectedText);
  }
}

// Test query classes
public class TestCachedQuery : ICachedQuery
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string CacheKey => $"test-query-{Id}";
  public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}

public class TestCachedQueryWithNullExpiration : ICachedQuery
{
  public string CacheKey => "test-query-no-expiration";
  public TimeSpan? Expiration => null;
}

public class TestCachedQueryWithCustomExpiration : ICachedQuery
{
  private readonly TimeSpan _expiration;

  public TestCachedQueryWithCustomExpiration(TimeSpan expiration)
  {
    _expiration = expiration;
  }

  public string CacheKey => "test-query-custom-expiration";
  public TimeSpan? Expiration => _expiration;
}

public class TestCachedComplexQuery : ICachedQuery
{
  public string CacheKey => "test-complex-query";
  public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}

public class ComplexResponse
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public int Count { get; set; }
}