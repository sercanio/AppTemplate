using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Domain.Roles.DomainEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Commands.Create;

[Trait("Category", "Unit")]
public class CreateRoleEventHandlerUnitTests
{
  private readonly Mock<IRolesRepository> _rolesRepositoryMock;
  private readonly Mock<IEmailSender> _emailSenderMock;
  private readonly Mock<INotificationService> _notificationServiceMock;
  private readonly Mock<ILogger<CreateRoleCommandHander>> _loggerMock;
  private readonly CreateRoleEventHandler _handler;

  public CreateRoleEventHandlerUnitTests()
  {
    _rolesRepositoryMock = new Mock<IRolesRepository>();
    _emailSenderMock = new Mock<IEmailSender>();
    _notificationServiceMock = new Mock<INotificationService>();
    _loggerMock = new Mock<ILogger<CreateRoleCommandHander>>();

    _handler = new CreateRoleEventHandler(
        _rolesRepositoryMock.Object,
        _emailSenderMock.Object,
        _notificationServiceMock.Object,
        _loggerMock.Object);
  }

  [Fact]
  public async Task Handle_ShouldLogInformation_WhenEventIsHandled()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleCreatedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains("Handling RoleCreatedDomainEvent") &&
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
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
  public async Task Handle_ShouldNotCallRepository_AsItOnlyLogs()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleCreatedDomainEvent(roleId);
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(domainEvent, cancellationToken);

    // Assert
    _rolesRepositoryMock.VerifyNoOtherCalls();
    _emailSenderMock.VerifyNoOtherCalls();
    _notificationServiceMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Handle_ShouldHandleMultipleEvents_Independently()
  {
    // Arrange
    var event1 = new RoleCreatedDomainEvent(Guid.NewGuid());
    var event2 = new RoleCreatedDomainEvent(Guid.NewGuid());
    var cancellationToken = CancellationToken.None;

    // Act
    await _handler.Handle(event1, cancellationToken);
    await _handler.Handle(event2, cancellationToken);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling RoleCreatedDomainEvent")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Exactly(2));
  }

  [Fact]
  public async Task Handle_ShouldComplete_WithEmptyGuid()
  {
    // Arrange
    var roleId = Guid.Empty;
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
    var handler = new CreateRoleEventHandler(
        _rolesRepositoryMock.Object,
        _emailSenderMock.Object,
        _notificationServiceMock.Object,
        _loggerMock.Object);

    // Assert
    handler.Should().NotBeNull();
  }

  [Fact]
  public async Task Handle_ShouldBeAsync_AndReturnCompletedTask()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
  public async Task Handle_ShouldLogCorrectly_ForVariousRoleIds(string roleIdString)
  {
    // Arrange
    var roleId = Guid.Parse(roleIdString);
    var domainEvent = new RoleCreatedDomainEvent(roleId);
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
}