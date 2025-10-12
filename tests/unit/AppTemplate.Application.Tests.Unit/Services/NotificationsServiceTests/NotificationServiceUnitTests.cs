using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace AppTemplate.Application.Tests.Unit.Services.NotificationsServiceTests;

[Trait("Category", "Unit")]
public class NotificationServiceUnitTests
{
  private readonly Mock<INotificationsRepository> _notificationsRepositoryMock;
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<IAppUsersService> _usersServiceMock;
  private readonly Mock<IRolesService> _rolesServiceMock;
  private readonly Mock<IMemoryCache> _cacheMock;
  private readonly Mock<ILogger<NotificationsService>> _loggerMock;
  private readonly NotificationsService _service;

  public NotificationServiceUnitTests()
  {
    _notificationsRepositoryMock = new Mock<INotificationsRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _usersServiceMock = new Mock<IAppUsersService>();
    _rolesServiceMock = new Mock<IRolesService>();
    _cacheMock = new Mock<IMemoryCache>();
    _loggerMock = new Mock<ILogger<NotificationsService>>();
    _service = new NotificationsService(
      _notificationsRepositoryMock.Object,
      _unitOfWorkMock.Object,
      _usersServiceMock.Object,
      _rolesServiceMock.Object,
      _cacheMock.Object,
      _loggerMock.Object
    );
  }

  #region NotificationException Tests

  [Fact]
  public void NotificationException_Constructor_WithMessage_ShouldCreateException()
  {
    // Arrange
    var expectedMessage = "Test notification error";

    // Act
    var exception = new NotificationException(expectedMessage);

    // Assert
    exception.Should().NotBeNull();
    exception.Message.Should().Be(expectedMessage);
    exception.InnerException.Should().BeNull();
  }

  [Fact]
  public void NotificationException_Constructor_WithMessageAndInnerException_ShouldCreateException()
  {
    // Arrange
    var expectedMessage = "Test notification error";
    var innerException = new InvalidOperationException("Inner error");

    // Act
    var exception = new NotificationException(expectedMessage, innerException);

    // Assert
    exception.Should().NotBeNull();
    exception.Message.Should().Be(expectedMessage);
    exception.InnerException.Should().Be(innerException);
    exception.InnerException.Should().BeOfType<InvalidOperationException>();
  }

  [Fact]
  public void NotificationException_ShouldBeThrowable()
  {
    // Arrange
    var message = "Throwable notification error";

    // Act
    Action act = () => throw new NotificationException(message);

    // Assert
    act.Should().Throw<NotificationException>()
        .WithMessage(message);
  }

  [Fact]
  public void NotificationException_ShouldBeCatchableAsException()
  {
    // Arrange
    var message = "Catchable notification error";

    // Act
    Action act = () => throw new NotificationException(message);

    // Assert
    act.Should().Throw<Exception>()
        .Which.Should().BeOfType<NotificationException>();
  }

  [Fact]
  public void NotificationException_WithInnerException_ShouldPreserveInnerExceptionMessage()
  {
    // Arrange
    var outerMessage = "Outer error";
    var innerMessage = "Inner error details";
    var innerException = new ArgumentNullException("param", innerMessage);

    // Act
    var exception = new NotificationException(outerMessage, innerException);

    // Assert
    exception.Message.Should().Be(outerMessage);
    exception.InnerException.Should().NotBeNull();
    exception.InnerException!.Message.Should().Contain(innerMessage);
  }

  [Fact]
  public void NotificationException_ShouldSupportExceptionSerialization()
  {
    // Arrange
    var message = "Serializable notification error";
    var exception = new NotificationException(message);

    // Act & Assert - Verify exception has standard exception properties
    exception.Message.Should().Be(message);
    exception.ToString().Should().Contain(message);
    exception.ToString().Should().Contain(nameof(NotificationException));
  }

  [Fact]
  public void NotificationException_WithNullMessage_ShouldNotThrow()
  {
    // Act
    Action act = () => new NotificationException(null!);

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void NotificationException_WithEmptyMessage_ShouldCreateExceptionWithEmptyMessage()
  {
    // Arrange
    var emptyMessage = string.Empty;

    // Act
    var exception = new NotificationException(emptyMessage);

    // Assert
    exception.Should().NotBeNull();
    exception.Message.Should().Be(emptyMessage);
  }

  [Fact]
  public void NotificationException_WithNullInnerException_ShouldCreateExceptionWithNullInner()
  {
    // Arrange
    var message = "Test message";

    // Act
    var exception = new NotificationException(message, null!);

    // Assert
    exception.Should().NotBeNull();
    exception.Message.Should().Be(message);
    exception.InnerException.Should().BeNull();
  }

  [Fact]
  public void NotificationException_StackTrace_ShouldBePopulatedWhenThrown()
  {
    // Arrange & Act
    NotificationException? caughtException = null;
    try
    {
      throw new NotificationException("Stack trace test");
    }
    catch (NotificationException ex)
    {
      caughtException = ex;
    }

    // Assert
    caughtException.Should().NotBeNull();
    caughtException!.StackTrace.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public void NotificationException_WithNestedInnerExceptions_ShouldPreserveExceptionChain()
  {
    // Arrange
    var innermost = new InvalidOperationException("Innermost");
    var middle = new ArgumentException("Middle", innermost);
    var outer = new NotificationException("Outer", middle);

    // Act & Assert
    outer.InnerException.Should().Be(middle);
    outer.InnerException!.InnerException.Should().Be(innermost);
  }

  [Theory]
  [InlineData("Simple error message")]
  [InlineData("Error with special characters: !@#$%^&*()")]
  [InlineData("Error with unicode: 你好世界")]
  [InlineData("Error with newline\nand tabs\t")]
  public void NotificationException_WithVariousMessages_ShouldPreserveMessage(string message)
  {
    // Act
    var exception = new NotificationException(message);

    // Assert
    exception.Message.Should().Be(message);
  }

  [Fact]
  public void NotificationException_ToString_ShouldIncludeExceptionType()
  {
    // Arrange
    var message = "Test exception";
    var exception = new NotificationException(message);

    // Act
    var result = exception.ToString();

    // Assert
    result.Should().Contain(nameof(NotificationException));
    result.Should().Contain(message);
  }

  [Fact]
  public void NotificationException_WithLongMessage_ShouldPreserveFullMessage()
  {
    // Arrange
    var longMessage = new string('A', 10000);

    // Act
    var exception = new NotificationException(longMessage);

    // Assert
    exception.Message.Should().Be(longMessage);
    exception.Message.Length.Should().Be(10000);
  }

  [Fact]
  public void NotificationException_CaughtInTryCatch_ShouldBeCatchable()
  {
    // Arrange
    var message = "Catchable error";
    var wasCaught = false;

    // Act
    try
    {
      throw new NotificationException(message);
    }
    catch (NotificationException ex)
    {
      wasCaught = true;
      ex.Message.Should().Be(message);
    }

    // Assert
    wasCaught.Should().BeTrue();
  }

  [Fact]
  public void NotificationException_AsBaseException_ShouldRetainType()
  {
    // Arrange
    Exception baseException = new NotificationException("Test");

    // Act & Assert
    baseException.Should().BeOfType<NotificationException>();
    (baseException is NotificationException).Should().BeTrue();
  }

  #endregion

  #region Existing Tests

  [Fact]
  public async Task AddAsync_CallsRepositoryAddAsync()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    await _service.AddAsync(notification);
    _notificationsRepositoryMock.Verify(r => r.AddAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public void Delete_CallsRepositoryDelete()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    _service.Delete(notification);
    _notificationsRepositoryMock.Verify(r => r.Delete(notification, true, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public void Update_CallsRepositoryUpdate()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    _service.Update(notification);
    _notificationsRepositoryMock.Verify(r => r.Update(notification, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAllAsync_ReturnsPaginatedList()
  {
    var paginatedList = new PaginatedList<Notification>(new List<Notification>(), 0, 0, 10);
    _notificationsRepositoryMock
      .Setup(r => r.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<Notification, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<Notification>, IQueryable<Notification>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedList);

    var result = await _service.GetAllAsync();

    Assert.NotNull(result);
    Assert.Equal(0, result.TotalCount);
  }

  [Fact]
  public async Task GetAsync_ReturnsNotification()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    _notificationsRepositoryMock
      .Setup(r => r.GetAsync(
        It.IsAny<Expression<Func<Notification, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<Notification>, IQueryable<Notification>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(notification);

    var result = await _service.GetAsync(n => n.Id == notification.Id);

    Assert.NotNull(result);
    Assert.Equal(notification, result);
  }

  [Fact]
  public async Task GetNotificationsByUserIdAsync_ReturnsPaginatedList()
  {
    var userId = Guid.NewGuid();
    var paginatedList = new PaginatedList<Notification>(new List<Notification>(), 0, 0, 10);
    _notificationsRepositoryMock
      .Setup(r => r.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<Notification, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<Notification>, IQueryable<Notification>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedList);

    var result = await _service.GetNotificationsByUserIdAsync(userId, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(0, result.TotalCount);
  }

  [Fact]
  public async Task GetUnreadCountAsync_ReturnsCount()
  {
    var userId = Guid.NewGuid();
    _notificationsRepositoryMock
      .Setup(r => r.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    var count = await _service.GetUnreadCountAsync(userId);

    Assert.Equal(0, count);
  }

  [Fact]
  public async Task GetUnreadNotificationsAsync_ReturnsList()
  {
    var userId = Guid.NewGuid();
    var notifications = new List<Notification>
    {
      new Notification(userId, "title", "msg", NotificationTypeEnum.System)
    };
    _notificationsRepositoryMock
      .Setup(r => r.GetUnreadNotificationsAsync(userId, 0, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync(notifications);

    var result = await _service.GetUnreadNotificationsAsync(userId);

    Assert.Single(result);
  }

  [Fact]
  public async Task MarkNotificationAsReadAsync_CallsRepository()
  {
    var notificationId = Guid.NewGuid();
    _notificationsRepositoryMock
      .Setup(r => r.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    var result = await _service.MarkNotificationAsReadAsync(notificationId);

    Assert.True(result);
  }

  [Fact]
  public async Task MarkNotificationsAsReadAsync_CallsRepository()
  {
    var userId = Guid.NewGuid();
    _notificationsRepositoryMock
      .Setup(r => r.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask)
      .Verifiable();

    await _service.MarkNotificationsAsReadAsync(userId, CancellationToken.None);

    _notificationsRepositoryMock.Verify(r => r.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendNotificationAsync_AddsAndSavesNotification()
  {
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask)
      .Verifiable();
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0)
      .Verifiable();

    await _service.SendNotificationAsync("title", "msg");

    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserAsync_UserNotFound_LogsWarning()
  {
    var userId = Guid.NewGuid();
    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((AppUser?)null);

    await _service.SendNotificationToUserAsync("title", "msg", NotificationTypeEnum.System, userId);

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserAsync_UserFound_AddsAndSavesNotification()
  {
    var userId = Guid.NewGuid();
    var user = AppUser.Create();
    user.SetIdentityId("id");
    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask)
      .Verifiable();
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0)
      .Verifiable();

    await _service.SendNotificationToUserAsync("title", "msg", NotificationTypeEnum.System, userId);

    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SaveNotificationAsync_UserNotFound_LogsWarning()
  {
    var userId = Guid.NewGuid();
    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((AppUser?)null);

    await _service.SaveNotificationAsync("title", "msg", NotificationTypeEnum.System, userId);

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
        Times.Once);
  }

  [Fact]
  public async Task SaveNotificationAsync_UserFound_AddsAndSavesNotification()
  {
    var userId = Guid.NewGuid();
    var user = AppUser.Create();
    user.SetIdentityId("id");
    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask)
      .Verifiable();
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0)
      .Verifiable();

    await _service.SaveNotificationAsync("title", "msg", NotificationTypeEnum.System, userId);

    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion
}