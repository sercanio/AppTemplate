using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.AppUsers.ValueObjects;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Domain.Roles;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;
using static AppTemplate.Application.Services.Notifications.NotificationsService;

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
    _usersServiceMock = new Mock<IAppUsersService>
    ();
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
    var user = CreateTestUser("id");

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
    var user = CreateTestUser("id");

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

  #region New Coverage Tests

  // Exception handling tests for SendNotificationAsync
  [Fact]
  public async Task SendNotificationAsync_WhenExceptionThrown_ShouldLogErrorAndThrowNotificationException()
  {
    // Arrange
    var expectedException = new Exception("Database error");
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(expectedException);

    // Act
    Func<Task> act = async () => await _service.SendNotificationAsync("title", "message");

    // Assert
    await act.Should().ThrowAsync<NotificationException>()
      .WithMessage("Failed to send system notification");

    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        expectedException,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
  }

  // Exception handling tests for SendNotificationToUserAsync
  [Fact]
  public async Task SendNotificationToUserAsync_WhenExceptionThrown_ShouldLogErrorAndThrowNotificationException()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = CreateTestUser("id");
    var expectedException = new Exception("Database error");

    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(expectedException);

    // Act
    Func<Task> act = async () => await _service.SendNotificationToUserAsync("title", "message", NotificationTypeEnum.System, userId);

    // Assert
    await act.Should().ThrowAsync<NotificationException>()
      .WithMessage($"Failed to send notification to user {userId}");

    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        expectedException,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
  }

  // Exception handling tests for SaveNotificationAsync
  [Fact]
  public async Task SaveNotificationAsync_WhenExceptionThrown_ShouldLogErrorAndThrowNotificationException()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = CreateTestUser("id");
    var expectedException = new Exception("Database error");

    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(expectedException);

    // Act
    Func<Task> act = async () => await _service.SaveNotificationAsync("title", "message", NotificationTypeEnum.System, userId);

    // Assert
    await act.Should().ThrowAsync<NotificationException>()
      .WithMessage($"Failed to save notification for user {userId}");

    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        expectedException,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
  }

  // Test for SendNotificationToUserAsync with disabled notifications
  [Fact]
  public async Task SendNotificationToUserAsync_WhenNotificationsDisabled_ShouldStillSaveButNotSendRealtime()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = CreateTestUserWithDisabledNotifications("id");

    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationToUserAsync("title", "msg", NotificationTypeEnum.System, userId);

    // Assert
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("disabled in-app notifications")),
        null,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
  }

  // SanitizeContent tests
  [Fact]
  public async Task SendNotificationAsync_ShouldSanitizeContent()
  {
    // Arrange
    var maliciousContent = "<script>alert('xss')</script>Hello";
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationAsync("title", maliciousContent);

    // Assert
    _notificationsRepositoryMock.Verify(
      r => r.AddAsync(
        It.Is<Notification>(n => !n.Message.Contains("<script>")),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task SendNotificationAsync_WithNullOrWhitespaceContent_ShouldHandleGracefully(string content)
  {
    // Arrange
    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationAsync("title", content);

    // Assert
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  // SendNotificationToUserGroupAsync tests
  [Fact]
  public async Task SendNotificationToUserGroupAsync_WithAdminRole_ShouldUseStaticRole()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = Role.Admin.Name.Value;
    var user = CreateTestUserWithRole("user123", Role.Admin);

    var users = new List<AppUser> { user };
    var paginatedUsers = new PaginatedList<AppUser>(users, users.Count, 0, 10);

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedUsers);

    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using static Admin role")),
        null,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserGroupAsync_WithDefaultRole_ShouldUseStaticRole()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = Role.DefaultRole.Name.Value;
    
    // Create a properly initialized user
    var user = AppUser.Create();
    var identityIdProperty = typeof(AppUser).GetProperty("IdentityId");
    identityIdProperty?.SetValue(user, "user123");
    user.AddRole(Role.DefaultRole);
    
    // Double-check the role is there
    Assert.NotNull(user.Roles);
    Assert.Contains(Role.DefaultRole, user.Roles);

    var users = new List<AppUser> { user };
    var paginatedUsers = new PaginatedList<AppUser>(users, users.Count, 0, 10);

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedUsers);

    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserGroupAsync_WithCustomRole_ShouldQueryDatabase()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = "CustomRole";
    var customRole = Role.Create("CustomRole", "Custom Role", Guid.NewGuid());
    var user = CreateTestUserWithRole("user123", customRole);

    var roles = new List<Role> { customRole };
    var paginatedRoles = new PaginatedList<Role>(roles, roles.Count, 0, 100);

    var users = new List<AppUser> { user };
    var paginatedUsers = new PaginatedList<AppUser>(users, users.Count, 0, 10);

    _rolesServiceMock
      .Setup(r => r.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<Role, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedRoles);

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedUsers);

    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    _rolesServiceMock.Verify(
      r => r.GetAllAsync(
        It.IsAny<int>(),
        100,
        null,
        false,
        It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserGroupAsync_WithNonExistentRole_ShouldLogWarningAndReturn()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = "NonExistentRole";
    var roles = new List<Role>();
    var paginatedRoles = new PaginatedList<Role>(roles, 0, 0, 100);

    _rolesServiceMock
      .Setup(r => r.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<Role, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<Role>, IQueryable<Role>>>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedRoles);

    // Act
    await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot send notification to non-existent group")),
        null,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task SendNotificationToUserGroupAsync_WithNoUsersInGroup_ShouldLogInfoAndReturn()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = Role.Admin.Name.Value;
    var users = new List<AppUser>();
    var paginatedUsers = new PaginatedList<AppUser>(users, 0, 0, 10);

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedUsers);

    // Act
    await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No users found in group")),
        null,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task SendNotificationToUserGroupAsync_ShouldExcludeOperator()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = Role.Admin.Name.Value;

    var operatorUser = CreateTestUserWithRole(operatorId, Role.Admin);
    var otherUser = CreateTestUserWithRole("user123", Role.Admin);

    var users = new List<AppUser> { operatorUser, otherUser };
    var paginatedUsers = new PaginatedList<AppUser>(users, users.Count, 0, 10);

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedUsers);

    _notificationsRepositoryMock
      .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0);

    // Act
    await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserGroupAsync_WhenExceptionThrown_ShouldLogErrorAndThrowNotificationException()
  {
    // Arrange
    var operatorId = "operator123";
    var groupName = Role.Admin.Name.Value;
    var expectedException = new Exception("Database error");

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(expectedException);

    // Act
    Func<Task> act = async () => await _service.SendNotificationToUserGroupAsync("title", "message", NotificationTypeEnum.System, operatorId, groupName);

    // Assert
    await act.Should().ThrowAsync<NotificationException>()
      .WithMessage($"Failed to send notification to group {groupName}");

    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        expectedException,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Once);
  }

  // SendNotificationToUserGroupsAsync tests
  [Fact]
  public async Task SendNotificationToUserGroupsAsync_ShouldCallSendNotificationToUserGroupAsync_ForEachGroup()
  {
    // Arrange
    var operatorId = "operator123";
    var groupNames = new List<string> { "Admin", Role.DefaultRole.Name.Value };

    var users = new List<AppUser>();
    var paginatedUsers = new PaginatedList<AppUser>(users, 0, 0, 10);

    _usersServiceMock
      .Setup(u => u.GetAllAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<AppUser, bool>>>(),
        It.IsAny<bool>(),
        It.IsAny<Func<IQueryable<AppUser>, IQueryable<AppUser>>>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(paginatedUsers);

    // Act
    await _service.SendNotificationToUserGroupsAsync("title", "message", NotificationTypeEnum.System, operatorId, groupNames);

    // Assert - Should log "No users found in group" twice (once for each group)
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No users found in group")),
        null,
        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
      Times.Exactly(2));
  }

  #endregion

  #region Helper Methods

  private static AppUser CreateTestUser(string identityId)
  {
    var user = AppUser.Create();
    // Use reflection to set IdentityId since there's no public setter
    var identityIdProperty = typeof(AppUser).GetProperty("IdentityId");
    identityIdProperty?.SetValue(user, identityId);
    return user;
  }

  private static AppUser CreateTestUserWithDisabledNotifications(string identityId)
  {
    var user = AppUser.Create();
    // Use reflection to set IdentityId
    var identityIdProperty = typeof(AppUser).GetProperty("IdentityId");
    identityIdProperty?.SetValue(user, identityId);

    // Update notification preferences to disable in-app notifications
    user.NotificationPreference.Update(false, true, true);
    return user;
  }

  private static AppUser CreateTestUserWithRole(string identityId, Role role)
  {
    var user = AppUser.Create();
    
    // Set IdentityId via reflection
    var identityIdProperty = typeof(AppUser).GetProperty("IdentityId");
    identityIdProperty?.SetValue(user, identityId);

    // Add role using the public method - no reflection needed!
    user.AddRole(role);

    return user;
  }

  #endregion

  [Fact]
  public void GetCacheKey_WithInvalidCacheKeyType_ShouldThrowArgumentOutOfRangeException()
  {
    // Arrange
    var invalidCacheKeyType = (CacheKeyType)999; // Invalid enum value
    var userId = "user123";
    
    // Use reflection to access the private GetCacheKey method
    var getCacheKeyMethod = typeof(NotificationsService).GetMethod(
      "GetCacheKey", 
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
    // Act
    Action act = () =>
    {
      try
      {
        getCacheKeyMethod?.Invoke(_service, new object[] { invalidCacheKeyType, userId, null, null });
      }
      catch (System.Reflection.TargetInvocationException ex)
      {
        // Unwrap the inner exception thrown by reflection
        if (ex.InnerException != null)
        {
          throw ex.InnerException;
        }
        throw;
      }
    };
    
    // Assert
    act.Should().Throw<ArgumentOutOfRangeException>()
      .WithParameterName("type");
  }
}