using AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;
using AppTemplate.Domain.Roles.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Update;

[Trait("Category", "Unit")]
public class UpdateRoleNameEventHandlerUnitTests
{
  private readonly Mock<ILogger<UpdateRoleNameEventHandler>> _loggerMock;
  private readonly UpdateRoleNameEventHandler _handler;

  public UpdateRoleNameEventHandlerUnitTests()
  {
    _loggerMock = new Mock<ILogger<UpdateRoleNameEventHandler>>();
    _handler = new UpdateRoleNameEventHandler(_loggerMock.Object);
  }

  [Fact]
  public async Task Handle_ShouldLogInformation_WhenEventIsHandled()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var oldRoleName = "OldAdmin";
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, oldRoleName);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains("Handling RoleNameUpdatedDomainEvent") &&
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
    var oldRoleName = "Admin";
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, oldRoleName);
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
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, "Admin");
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
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, "OldRoleName");
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
    var event1 = new RoleNameUpdatedDomainEvent(Guid.NewGuid(), "Admin");
    var event2 = new RoleNameUpdatedDomainEvent(Guid.NewGuid(), "User");
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(event1, cancellationToken);
    await _handler.Handle(event2, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling RoleNameUpdatedDomainEvent")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_ShouldComplete_WithEmptyGuid()
  {
    // Arrange
    var roleId = Guid.Empty;
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, "RoleName");
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
    var handler = new UpdateRoleNameEventHandler(_loggerMock.Object);

    // Assert
    handler.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_ShouldBeAsync_AndReturnCompletedTask()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, "Admin");
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
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, "Admin");
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

  [Theory]
  [InlineData("Admin")]
  [InlineData("User")]
  [InlineData("SuperAdministrator")]
  [InlineData("")]
  public async Task Handle_ShouldHandleVariousOldRoleNames(string oldRoleName)
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleNameUpdatedDomainEvent(roleId, oldRoleName);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }
}