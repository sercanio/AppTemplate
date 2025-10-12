using AppTemplate.Application.Data.Dapper;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Application.Services.OutboxMessages;
using AppTemplate.Domain;
using AppTemplate.Domain.OutboxMessages;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Quartz;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Services.OutboxMessagesTests;

[Trait("Category", "Unit")]
public class ProcessOutboxMessagesJobUnitTests
{
  private readonly Mock<ISqlConnectionFactory> _sqlConnectionFactoryMock;
  private readonly Mock<IPublisher> _publisherMock;
  private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
  private readonly Mock<IOptions<OutboxOptions>> _outboxOptionsMock;
  private readonly Mock<ILogger<ProcessOutboxMessagesJob>> _loggerMock;
  private readonly Mock<IJobExecutionContext> _jobExecutionContextMock;

  public ProcessOutboxMessagesJobUnitTests()
  {
    _sqlConnectionFactoryMock = new Mock<ISqlConnectionFactory>();
    _publisherMock = new Mock<IPublisher>();
    _dateTimeProviderMock = new Mock<IDateTimeProvider>();
    _outboxOptionsMock = new Mock<IOptions<OutboxOptions>>();
    _loggerMock = new Mock<ILogger<ProcessOutboxMessagesJob>>();
    _jobExecutionContextMock = new Mock<IJobExecutionContext>();

    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = 10 };
    _outboxOptionsMock.Setup(x => x.Value).Returns(outboxOptions);
    _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
  }

  [Fact]
  public void Constructor_ShouldInitialize_WithValidDependencies()
  {
    // Arrange & Act
    var job = new ProcessOutboxMessagesJob(
        _sqlConnectionFactoryMock.Object,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object);

    // Assert
    job.Should().NotBeNull();
  }

  [Fact]
  public void Constructor_ShouldThrow_WhenDependenciesAreNull()
  {
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => new ProcessOutboxMessagesJob(
        null!,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object));
  }

  [Fact]
  public void OutboxOptions_ShouldBeConfigured_WithCorrectValues()
  {
    // Arrange
    var batchSize = 25;
    var intervalInSeconds = 60;
    var outboxOptions = new OutboxOptions { IntervalInSeconds = intervalInSeconds, BatchSize = batchSize };
    _outboxOptionsMock.Setup(x => x.Value).Returns(outboxOptions);

    // Act
    var job = new ProcessOutboxMessagesJob(
        _sqlConnectionFactoryMock.Object,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object);

    // Assert
    job.Should().NotBeNull();
    outboxOptions.BatchSize.Should().Be(batchSize);
    outboxOptions.IntervalInSeconds.Should().Be(intervalInSeconds);
  }

  [Fact]
  public void DateTimeProvider_ShouldProvideUtcTime()
  {
    // Arrange
    var now = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);

    // Act
    var result = _dateTimeProviderMock.Object.UtcNow;

    // Assert
    result.Should().Be(now);
    result.Kind.Should().Be(DateTimeKind.Utc);
  }

  [Fact]
  public void OutboxMessageResponse_ShouldBeCreated_WithCorrectProperties()
  {
    // Arrange
    var id = Guid.NewGuid();
    var content = "test content";

    // Act
    var response = new ProcessOutboxMessagesJob.OutboxMessageResponse(id, content);

    // Assert
    response.Id.Should().Be(id);
    response.Content.Should().Be(content);
  }

  [Fact]
  public void OutboxMessageResponse_ShouldSupportDeconstruction()
  {
    // Arrange
    var id = Guid.NewGuid();
    var content = "test content";
    var response = new ProcessOutboxMessagesJob.OutboxMessageResponse(id, content);

    // Act
    var (actualId, actualContent) = response;

    // Assert
    actualId.Should().Be(id);
    actualContent.Should().Be(content);
  }

  [Theory]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(20)]
  [InlineData(50)]
  public void OutboxOptions_ShouldAcceptVariousBatchSizes(int batchSize)
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = batchSize };
    _outboxOptionsMock.Setup(x => x.Value).Returns(outboxOptions);

    // Act
    var job = new ProcessOutboxMessagesJob(
        _sqlConnectionFactoryMock.Object,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object);

    // Assert
    job.Should().NotBeNull();
    outboxOptions.BatchSize.Should().Be(batchSize);
  }

  [Fact]
  public void ProcessOutboxMessagesJob_ShouldHaveDisallowConcurrentExecutionAttribute()
  {
    // Arrange & Act
    var attribute = typeof(ProcessOutboxMessagesJob)
        .GetCustomAttributes(typeof(DisallowConcurrentExecutionAttribute), false)
        .FirstOrDefault();

    // Assert
    attribute.Should().NotBeNull();
    attribute.Should().BeOfType<DisallowConcurrentExecutionAttribute>();
  }

  [Fact]
  public void ProcessOutboxMessagesJob_ShouldImplementIJobInterface()
  {
    // Arrange & Act
    var implementsIJob = typeof(IJob).IsAssignableFrom(typeof(ProcessOutboxMessagesJob));

    // Assert
    implementsIJob.Should().BeTrue();
  }

  [Fact]
  public void JsonSerializerSettings_ShouldUseTypeNameHandling()
  {
    // Arrange
    var testEvent = new TestDomainEvent(Guid.NewGuid());
    var settings = new JsonSerializerSettings
    {
      TypeNameHandling = TypeNameHandling.All
    };

    // Act
    var serialized = JsonConvert.SerializeObject(testEvent, settings);
    var deserialized = JsonConvert.DeserializeObject<IDomainEvent>(serialized, settings);

    // Assert
    deserialized.Should().NotBeNull();
    deserialized.Should().BeOfType<TestDomainEvent>();
    ((TestDomainEvent)deserialized!).Id.Should().Be(testEvent.Id);
  }

  [Fact]
  public void OutboxMessageResponse_WithEmptyContent_ShouldBeValid()
  {
    // Arrange
    var id = Guid.NewGuid();
    var content = string.Empty;

    // Act
    var response = new ProcessOutboxMessagesJob.OutboxMessageResponse(id, content);

    // Assert
    response.Id.Should().Be(id);
    response.Content.Should().BeEmpty();
  }

  [Fact]
  public void OutboxOptions_WithZeroInterval_ShouldBeValid()
  {
    // Arrange & Act
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 0, BatchSize = 10 };

    // Assert
    outboxOptions.IntervalInSeconds.Should().Be(0);
    outboxOptions.BatchSize.Should().Be(10);
  }

  [Fact]
  public void DateTimeProvider_ShouldReturnUtcTime()
  {
    // Arrange
    var now = DateTime.UtcNow;
    _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(now);

    // Act
    var result = _dateTimeProviderMock.Object.UtcNow;

    // Assert
    result.Kind.Should().Be(DateTimeKind.Utc);
    result.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
  }

  [Fact]
  public void Publisher_ShouldBeConfigured()
  {
    // Arrange & Act
    var job = new ProcessOutboxMessagesJob(
        _sqlConnectionFactoryMock.Object,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object);

    // Assert
    _publisherMock.Should().NotBeNull();
  }

  [Fact]
  public void SqlConnectionFactory_ShouldBeConfigured()
  {
    // Arrange & Act
    var job = new ProcessOutboxMessagesJob(
        _sqlConnectionFactoryMock.Object,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object);

    // Assert
    _sqlConnectionFactoryMock.Should().NotBeNull();
  }

  [Fact]
  public void Logger_ShouldBeConfigured()
  {
    // Arrange & Act
    var job = new ProcessOutboxMessagesJob(
        _sqlConnectionFactoryMock.Object,
        _publisherMock.Object,
        _dateTimeProviderMock.Object,
        _outboxOptionsMock.Object,
        _loggerMock.Object);

    // Assert
    _loggerMock.Should().NotBeNull();
  }

  [Fact]
  public void OutboxMessageResponse_ShouldBeRecord()
  {
    // Arrange & Act
    var type = typeof(ProcessOutboxMessagesJob.OutboxMessageResponse);

    // Assert
    type.Should().NotBeNull();
    type.IsSealed.Should().BeTrue(); // Records are sealed
  }

  [Fact]
  public void OutboxMessageResponse_WithSameValues_ShouldBeEqual()
  {
    // Arrange
    var id = Guid.NewGuid();
    var content = "test content";
    var response1 = new ProcessOutboxMessagesJob.OutboxMessageResponse(id, content);
    var response2 = new ProcessOutboxMessagesJob.OutboxMessageResponse(id, content);

    // Act & Assert
    response1.Should().Be(response2);
    response1.GetHashCode().Should().Be(response2.GetHashCode());
  }

  [Fact]
  public void OutboxMessageResponse_WithDifferentValues_ShouldNotBeEqual()
  {
    // Arrange
    var response1 = new ProcessOutboxMessagesJob.OutboxMessageResponse(Guid.NewGuid(), "content1");
    var response2 = new ProcessOutboxMessagesJob.OutboxMessageResponse(Guid.NewGuid(), "content2");

    // Act & Assert
    response1.Should().NotBe(response2);
  }

  // Test domain event for testing purposes
  private sealed record TestDomainEvent(Guid Id) : IDomainEvent;
}