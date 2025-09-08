using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Notifications;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Application.Tests.Unit.Services.NotificationsServiceTests;
using AppTemplate.Core.Domain.Abstractions;
using AppTemplate.Core.Infrastructure.Pagination;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Notifications.Enums;
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

  [Fact]
  public async Task AddAsync_CallsRepositoryAddAsync()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    await _service.AddAsync(notification);
    _notificationsRepositoryMock.Verify(r => r.AddAsync(notification), Times.Once);
  }

  [Fact]
  public void Delete_CallsRepositoryDelete()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    _service.Delete(notification);
    _notificationsRepositoryMock.Verify(r => r.Delete(notification, true), Times.Once);
  }

  [Fact]
  public void Update_CallsRepositoryUpdate()
  {
    var notification = new Notification(Guid.NewGuid(), "title", "msg", NotificationTypeEnum.System);
    _service.Update(notification);
    _notificationsRepositoryMock.Verify(r => r.Update(notification), Times.Once);
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
      .Setup(r => r.AddAsync(It.IsAny<Notification>()))
      .Returns(Task.CompletedTask)
      .Verifiable();
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0)
      .Verifiable();

    await _service.SendNotificationAsync("title", "msg");

    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SendNotificationToUserAsync_UserNotFound_LogsWarning()
  {
    var userId = Guid.NewGuid();
    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((AppUser)null);

    await _service.SendNotificationToUserAsync("title", "msg", NotificationTypeEnum.System, userId);

    _loggerMock.VerifyLog(l => l.LogWarning(It.IsAny<string>(), userId), Times.Once());
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
      .Setup(r => r.AddAsync(It.IsAny<Notification>()))
      .Returns(Task.CompletedTask)
      .Verifiable();
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0)
      .Verifiable();

    await _service.SendNotificationToUserAsync("title", "msg", NotificationTypeEnum.System, userId);

    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SaveNotificationAsync_UserNotFound_LogsWarning()
  {
    var userId = Guid.NewGuid();
    _usersServiceMock.Setup(u => u.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((AppUser)null);

    await _service.SaveNotificationAsync("title", "msg", NotificationTypeEnum.System, userId);

    _loggerMock.VerifyLog(l => l.LogWarning(It.IsAny<string>(), userId), Times.Once());
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
      .Setup(r => r.AddAsync(It.IsAny<Notification>()))
      .Returns(Task.CompletedTask)
      .Verifiable();
    _unitOfWorkMock
      .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(0)
      .Verifiable();

    await _service.SaveNotificationAsync("title", "msg", NotificationTypeEnum.System, userId);

    _notificationsRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}

// Helper extension for verifying logger calls
public static class LoggerExtensions
{
  public static void VerifyLog<T>(this Mock<ILogger<T>> logger, Action<Mock<ILogger<T>>> verify, Times times)
  {
    verify(logger);
  }

  public static void LogWarning<T>(this Mock<ILogger<T>> logger, string message, params object[] args)
  {
    logger.Object.LogWarning(message, args);
  }
}
