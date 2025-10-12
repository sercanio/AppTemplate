using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Users.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Commands.UpdateUserRolesTests;

[Trait("Category", "Unit")]
public class RemoveUserRoleEventHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock;
  private readonly Mock<IRolesService> _rolesServiceMock;
  private readonly Mock<ILogger<RemoveUserRoleEventHandler>> _loggerMock;
  private readonly RemoveUserRoleEventHandler _handler;

  public RemoveUserRoleEventHandlerUnitTests()
  {
    _userRepositoryMock = new Mock<IAppUsersRepository>();
    _rolesServiceMock = new Mock<IRolesService>();
    _loggerMock = new Mock<ILogger<RemoveUserRoleEventHandler>>();

    _handler = new RemoveUserRoleEventHandler(
        _userRepositoryMock.Object,
        _rolesServiceMock.Object,
        _loggerMock.Object);
  }

  [Fact]
  public async Task Handle_ShouldLogInformation_WhenEventIsHandled()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains("Handling AppUserRoleRemovedDomainEvent") &&
                v.ToString()!.Contains(userId.ToString()) &&
                v.ToString()!.Contains(roleId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldCompleteSuccessfully_WithValidEvent()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
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
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.Cancel();

    // Act
    Func<Task> act = async () => await _handler.Handle(domainEvent, cancellationTokenSource.Token);

    // Assert
    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task Handle_ShouldLogCorrectUserIdAndRoleId()
  {
    // Arrange
    var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var roleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains(userId.ToString()) &&
                v.ToString()!.Contains(roleId.ToString())),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldNotCallRepository_AsItOnlyLogs()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _userRepositoryMock.VerifyNoOtherCalls();
    _rolesServiceMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Handle_ShouldHandleMultipleEvents_Independently()
  {
    // Arrange
    var event1 = new AppUserRoleRemovedDomainEvent(Guid.NewGuid(), Guid.NewGuid());
    var event2 = new AppUserRoleRemovedDomainEvent(Guid.NewGuid(), Guid.NewGuid());
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(event1, cancellationToken);
    await _handler.Handle(event2, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling AppUserRoleRemovedDomainEvent")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_ShouldComplete_WithEmptyGuidIds()
  {
    // Arrange
    var userId = Guid.Empty;
    var roleId = Guid.Empty;
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
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
    var handler = new RemoveUserRoleEventHandler(
        _userRepositoryMock.Object,
        _rolesServiceMock.Object,
        _loggerMock.Object);

    // Assert
    handler.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_ShouldBeAsync_AndReturnCompletedTask()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
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
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var domainEvent = new AppUserRoleRemovedDomainEvent(userId, roleId);
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
}