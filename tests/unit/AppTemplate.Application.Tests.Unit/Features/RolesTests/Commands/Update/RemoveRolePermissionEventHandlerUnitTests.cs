using AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;
using AppTemplate.Domain.Roles.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Update;

[Trait("Category", "Unit")]
public class RemoveRolePermissionEventHandlerUnitTests
{
  private readonly Mock<ILogger<RemoveRolePermissionEventHandler>> _loggerMock;
  private readonly RemoveRolePermissionEventHandler _handler;

  public RemoveRolePermissionEventHandlerUnitTests()
  {
    _loggerMock = new Mock<ILogger<RemoveRolePermissionEventHandler>>();
    _handler = new RemoveRolePermissionEventHandler(_loggerMock.Object);
  }

  [Fact]
  public async Task Handle_ShouldLogInformation_WhenEventIsHandled()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains("Handling RolePermissionRemovedDomainEvent") &&
                v.ToString()!.Contains(roleId.ToString()) &&
                v.ToString()!.Contains(permissionId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldCompleteSuccessfully_WithValidEvent()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
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
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.Cancel();

    // Act
    Func<Task> act = async () => await _handler.Handle(domainEvent, cancellationTokenSource.Token);

    // Assert
    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task Handle_ShouldLogCorrectRoleIdAndPermissionId()
  {
    // Arrange
    var roleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var permissionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains(roleId.ToString()) &&
                v.ToString()!.Contains(permissionId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldHandleMultipleEvents_Independently()
  {
    // Arrange
    var event1 = new RolePermissionRemovedDomainEvent(Guid.NewGuid(), Guid.NewGuid());
    var event2 = new RolePermissionRemovedDomainEvent(Guid.NewGuid(), Guid.NewGuid());
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(event1, cancellationToken);
    await _handler.Handle(event2, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling RolePermissionRemovedDomainEvent")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_ShouldComplete_WithEmptyGuids()
  {
    // Arrange
    var roleId = Guid.Empty;
    var permissionId = Guid.Empty;
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
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
    var handler = new RemoveRolePermissionEventHandler(_loggerMock.Object);

    // Assert
    handler.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_ShouldBeAsync_AndReturnCompletedTask()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
    var cancellationToken = CancellationToken.None;

    // Act
    var task = _handler.Handle(domainEvent, cancellationToken);

    // Assert
    task.Should().NotBeNull();
    await task;
    task.IsCompleted.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldOnlyLog_NoSideEffects()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
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
  public async Task Handle_ShouldFollowMediatRNotificationPattern()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            It.IsAny<LogLevel>(),
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
    var permissionId = Guid.NewGuid();
    var domainEvent = new RolePermissionRemovedDomainEvent(roleId, permissionId);
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
}