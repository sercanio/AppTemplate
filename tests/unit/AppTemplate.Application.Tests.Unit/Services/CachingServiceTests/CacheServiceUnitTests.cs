using System.Text.Json;
using AppTemplate.Application.Services.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Services.CachingServiceTests;

public class CacheServiceUnitTests
{
  private readonly Mock<IDistributedCache> _distributedCacheMock;
  private readonly CacheService _cacheService;

  public CacheServiceUnitTests()
  {
    _distributedCacheMock = new Mock<IDistributedCache>();
    _cacheService = new CacheService(_distributedCacheMock.Object);
  }

  [Fact]
  public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
  {
    // Arrange
    string key = "test-key";
    var testObject = new TestData { Id = 1, Name = "Test" };
    byte[] serializedData = JsonSerializer.SerializeToUtf8Bytes(testObject);

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync(serializedData);

    // Act
    var result = await _cacheService.GetAsync<TestData>(key);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(testObject.Id);
    result.Name.Should().Be(testObject.Name);
    _distributedCacheMock.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
  {
    // Arrange
    string key = "non-existent-key";

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync((byte[]?)null);

    // Act
    var result = await _cacheService.GetAsync<TestData>(key);

    // Assert
    result.Should().BeNull();
    _distributedCacheMock.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAsync_WithPrimitiveType_ShouldReturnValue()
  {
    // Arrange
    string key = "int-key";
    int expectedValue = 42;
    byte[] serializedData = JsonSerializer.SerializeToUtf8Bytes(expectedValue);

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync(serializedData);

    // Act
    var result = await _cacheService.GetAsync<int>(key);

    // Assert
    result.Should().Be(expectedValue);
  }

  [Fact]
  public async Task GetAsync_WithString_ShouldReturnValue()
  {
    // Arrange
    string key = "string-key";
    string expectedValue = "Hello World";
    byte[] serializedData = JsonSerializer.SerializeToUtf8Bytes(expectedValue);

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync(serializedData);

    // Act
    var result = await _cacheService.GetAsync<string>(key);

    // Assert
    result.Should().Be(expectedValue);
  }

  [Fact]
  public async Task GetAsync_WithComplexObject_ShouldReturnDeserializedObject()
  {
    // Arrange
    string key = "complex-key";
    var complexObject = new ComplexTestData
    {
      Id = 1,
      Name = "Complex",
      CreatedAt = DateTime.UtcNow,
      Tags = new List<string> { "tag1", "tag2" },
      Metadata = new Dictionary<string, string> { { "key1", "value1" } }
    };
    byte[] serializedData = JsonSerializer.SerializeToUtf8Bytes(complexObject);

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync(serializedData);

    // Act
    var result = await _cacheService.GetAsync<ComplexTestData>(key);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(complexObject.Id);
    result.Name.Should().Be(complexObject.Name);
    result.Tags.Should().BeEquivalentTo(complexObject.Tags);
    result.Metadata.Should().BeEquivalentTo(complexObject.Metadata);
  }

  [Fact]
  public async Task GetAsync_WithCancellationToken_ShouldPassTokenToCache()
  {
    // Arrange
    string key = "test-key";
    var cancellationToken = new CancellationToken();

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, cancellationToken))
        .ReturnsAsync((byte[]?)null);

    // Act
    await _cacheService.GetAsync<TestData>(key, cancellationToken);

    // Assert
    _distributedCacheMock.Verify(x => x.GetAsync(key, cancellationToken), Times.Once);
  }

  [Fact]
  public async Task SetAsync_WithObject_ShouldSerializeAndStoreInCache()
  {
    // Arrange
    string key = "test-key";
    var testObject = new TestData { Id = 1, Name = "Test" };
    byte[]? capturedBytes = null;
    DistributedCacheEntryOptions? capturedOptions = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) =>
            {
              capturedBytes = bytes;
              capturedOptions = options;
            })
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, testObject);

    // Assert
    capturedBytes.Should().NotBeNull();
    var deserializedObject = JsonSerializer.Deserialize<TestData>(capturedBytes!);
    deserializedObject.Should().BeEquivalentTo(testObject);
    _distributedCacheMock.Verify(
        x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
        Times.Once);
  }

  [Fact]
  public async Task SetAsync_WithExpiration_ShouldSetExpirationInOptions()
  {
    // Arrange
    string key = "test-key";
    var testObject = new TestData { Id = 1, Name = "Test" };
    var expiration = TimeSpan.FromMinutes(30);
    DistributedCacheEntryOptions? capturedOptions = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => capturedOptions = options)
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, testObject, expiration);

    // Assert
    capturedOptions.Should().NotBeNull();
    capturedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(expiration);
  }

  [Fact]
  public async Task SetAsync_WithoutExpiration_ShouldUseDefaultOptions()
  {
    // Arrange
    string key = "test-key";
    var testObject = new TestData { Id = 1, Name = "Test" };
    DistributedCacheEntryOptions? capturedOptions = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => capturedOptions = options)
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, testObject);

    // Assert
    capturedOptions.Should().NotBeNull();
  }

  [Fact]
  public async Task SetAsync_WithPrimitiveType_ShouldSerializeCorrectly()
  {
    // Arrange
    string key = "int-key";
    int value = 42;
    byte[]? capturedBytes = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => capturedBytes = bytes)
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, value);

    // Assert
    capturedBytes.Should().NotBeNull();
    var deserializedValue = JsonSerializer.Deserialize<int>(capturedBytes!);
    deserializedValue.Should().Be(value);
  }

  [Fact]
  public async Task SetAsync_WithComplexObject_ShouldSerializeCorrectly()
  {
    // Arrange
    string key = "complex-key";
    var complexObject = new ComplexTestData
    {
      Id = 1,
      Name = "Complex",
      CreatedAt = DateTime.UtcNow,
      Tags = new List<string> { "tag1", "tag2" },
      Metadata = new Dictionary<string, string> { { "key1", "value1" } }
    };
    byte[]? capturedBytes = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => capturedBytes = bytes)
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, complexObject);

    // Assert
    capturedBytes.Should().NotBeNull();
    var deserializedObject = JsonSerializer.Deserialize<ComplexTestData>(capturedBytes!);
    deserializedObject.Should().BeEquivalentTo(complexObject, options => options
        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
        .WhenTypeIs<DateTime>());
  }

  [Fact]
  public async Task SetAsync_WithCancellationToken_ShouldPassTokenToCache()
  {
    // Arrange
    string key = "test-key";
    var testObject = new TestData { Id = 1, Name = "Test" };
    var cancellationToken = new CancellationToken();

    _distributedCacheMock
        .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), cancellationToken))
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, testObject, null, cancellationToken);

    // Assert
    _distributedCacheMock.Verify(
        x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), cancellationToken),
        Times.Once);
  }

  [Fact]
  public async Task RemoveAsync_ShouldCallDistributedCacheRemove()
  {
    // Arrange
    string key = "test-key";

    _distributedCacheMock
        .Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.RemoveAsync(key);

    // Assert
    _distributedCacheMock.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task RemoveAsync_WithCancellationToken_ShouldPassTokenToCache()
  {
    // Arrange
    string key = "test-key";
    var cancellationToken = new CancellationToken();

    _distributedCacheMock
        .Setup(x => x.RemoveAsync(key, cancellationToken))
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.RemoveAsync(key, cancellationToken);

    // Assert
    _distributedCacheMock.Verify(x => x.RemoveAsync(key, cancellationToken), Times.Once);
  }

  [Fact]
  public async Task SetAsync_ThenGetAsync_ShouldReturnSameObject()
  {
    // Arrange
    string key = "test-key";
    var testObject = new TestData { Id = 1, Name = "Test" };
    byte[]? storedBytes = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => storedBytes = bytes)
        .Returns(Task.CompletedTask);

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync(() => storedBytes);

    // Act
    await _cacheService.SetAsync(key, testObject);
    var result = await _cacheService.GetAsync<TestData>(key);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEquivalentTo(testObject);
  }

  [Fact]
  public async Task GetAsync_WithNullBytes_ShouldReturnDefaultForValueType()
  {
    // Arrange
    string key = "int-key";

    _distributedCacheMock
        .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
        .ReturnsAsync((byte[]?)null);

    // Act
    var result = await _cacheService.GetAsync<int>(key);

    // Assert
    result.Should().Be(0); // Default value for int
  }

  [Fact]
  public async Task SetAsync_WithNullObject_ShouldSerializeNull()
  {
    // Arrange
    string key = "null-key";
    TestData? nullObject = null;
    byte[]? capturedBytes = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => capturedBytes = bytes)
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, nullObject);

    // Assert
    capturedBytes.Should().NotBeNull();
    var deserializedValue = JsonSerializer.Deserialize<TestData?>(capturedBytes!);
    deserializedValue.Should().BeNull();
  }

  [Fact]
  public async Task SetAsync_WithListOfObjects_ShouldSerializeCorrectly()
  {
    // Arrange
    string key = "list-key";
    var list = new List<TestData>
        {
            new TestData { Id = 1, Name = "Test1" },
            new TestData { Id = 2, Name = "Test2" }
        };
    byte[]? capturedBytes = null;

    _distributedCacheMock
        .Setup(x => x.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
        .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
            (k, bytes, options, ct) => capturedBytes = bytes)
        .Returns(Task.CompletedTask);

    // Act
    await _cacheService.SetAsync(key, list);

    // Assert
    capturedBytes.Should().NotBeNull();
    var deserializedList = JsonSerializer.Deserialize<List<TestData>>(capturedBytes!);
    deserializedList.Should().BeEquivalentTo(list);
  }

  // Test helper classes
  private class TestData
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  private class ComplexTestData
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
  }
}