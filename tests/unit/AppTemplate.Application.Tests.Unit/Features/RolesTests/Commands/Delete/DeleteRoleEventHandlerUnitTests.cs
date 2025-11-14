using AppTemplate.Application.Features.Roles.Commands.Delete;
using AppTemplate.Domain.Roles.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Delete;

[Trait("Category", "Unit")]
public class DeleteRoleEventHandlerUnitTests
{
  private readonly Mock<ILogger<DeleteRoleEventHandler>> _loggerMock;
  private readonly DeleteRoleEventHandler _handler;

  public DeleteRoleEventHandlerUnitTests()
  {
    _loggerMock = new Mock<ILogger<DeleteRoleEventHandler>>();
    _handler = new DeleteRoleEventHandler(_loggerMock.Object);
  }

  [Fact]
  public async Task Handle_ShouldLogInformation_WhenEventIsHandled()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains("Handling RoleDeletedDomainEvent") &&
                v.ToString()!.Contains(roleId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldCompleteSuccessfully_WithValidEvent()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    Func<Task> act = async () => await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task Handle_ShouldNotThrow_WhenCancellationTokenIsCancelled()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.Cancel();

    // Act
    Func<Task> act = async () => await _handler.Handle(domainEvent, cancellationTokenSource.Token);

    // Assert
    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task Handle_ShouldLogCorrectRoleId()
  {
    // Arrange
    var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(roleId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldHandleMultipleEvents_Independently()
  {
    // Arrange
    var event1 = new RoleDeletedDomainEvent(Guid.NewGuid());
    var event2 = new RoleDeletedDomainEvent(Guid.NewGuid());
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(event1, cancellationToken);
    await _handler.Handle(event2, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling RoleDeletedDomainEvent")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_ShouldComplete_WithEmptyGuid()
  {
    // Arrange
    var roleId = Guid.Empty;
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    Func<Task> act = async () => await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    await act.Should().NotThrowAsync();
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(Guid.Empty.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public void Constructor_ShouldInitializeHandler_WithValidDependencies()
  {
    // Arrange & Act
    var handler = new DeleteRoleEventHandler(_loggerMock.Object);

    // Assert
    handler.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_ShouldBeAsync_AndReturnCompletedTask()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    var task = _handler.Handle(domainEvent, cancellationToken);

    // Assert
    task.Should().NotBeNull();
    await task;
    task.IsCompleted.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldFollowMediatRNotificationPattern()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    // Should complete without exceptions and log appropriately
    _loggerMock.Verify(
        x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Theory]
  [InlineData("00000000-0000-0000-0000-000000000000")]
  [InlineData("12345678-1234-1234-1234-123456789012")]
  [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")]
  [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
  public async Task Handle_ShouldLogCorrectly_ForVariousRoleIds(string roleIdString)
  {
    // Arrange
    var roleId = Guid.Parse(roleIdString);
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(roleId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldLogWithCorrectLogLevel()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information, // Specifically Information level
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldNotLogError_WhenProcessingSucceeds()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldNotLogWarning_WhenProcessingSucceeds()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never);
  }

  [Fact]
  public async Task Handle_ShouldOnlyLog_NoSideEffects()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    // Verify only logging occurred, no other calls
    _loggerMock.Verify(
        x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    _loggerMock.VerifyNoOtherCalls();
  }

  [Fact]
  public void Constructor_ShouldAcceptLogger_AndStoreIt()
  {
    // Arrange
    var logger = new Mock<ILogger<DeleteRoleEventHandler>>();

    // Act
    var handler = new DeleteRoleEventHandler(logger.Object);

    // Assert
    handler.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_ShouldCompleteQuickly_AsItOnlyLogs()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    var startTime = DateTime.UtcNow;
    await _handler.Handle(domainEvent, cancellationToken);
    var duration = DateTime.UtcNow - startTime;

    // Assert
    duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100)); // Should complete very quickly
  }

  [Fact]
  public async Task Handle_ShouldBeIdempotent_WhenCalledMultipleTimes()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleDeletedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);
    await _handler.Handle(domainEvent, cancellationToken);
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(roleId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Exactly(3));
  }
}
