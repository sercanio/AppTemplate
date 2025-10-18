using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using AppTemplate.Application.Features.Notifications.Commands.MarkNotificationsAsRead;
using AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Domain.Notifications.Enums;
using AppTemplate.Web.Controllers.Api.v1;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Web.Tests.Unit.ControllersTests;

public class NotificationsControllerUnitTests
{
    private readonly Mock<ISender> _mockSender;
    private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
    private readonly NotificationsController _controller;

    public NotificationsControllerUnitTests()
    {
        _mockSender = new Mock<ISender>();
        _mockErrorHandlingService = new Mock<IErrorHandlingService>();

        _controller = new NotificationsController(
            _mockSender.Object,
            _mockErrorHandlingService.Object);

        SetupControllerContext();
    }

    private void SetupControllerContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAllNotifications_WithSuccessfulResult_ReturnsOkResult()
    {
        // Arrange
        var pageIndex = 0;
        var pageSize = 10;
        var cancellationToken = CancellationToken.None;

        var notifications = new List<GetAllNotificationsQueryResponse>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Test Notification", "Test message", NotificationTypeEnum.System, DateTime.UtcNow, false)
        };

        var paginatedList = new PaginatedList<GetAllNotificationsQueryResponse>(notifications, 1, pageIndex, pageSize);
        var queryResponse = new GetAllNotificationsWithUnreadCountResponse(paginatedList, 1);
        var result = Result.Success(queryResponse);

        _mockSender.Setup(x => x.Send(It.Is<GetAllNotificationsQuery>(q => 
            q.PageIndex == pageIndex && 
            q.PageSize == pageSize), cancellationToken))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetAllNotifications(pageIndex, pageSize, cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(queryResponse, okResult.Value);
        _mockSender.Verify(x => x.Send(It.IsAny<GetAllNotificationsQuery>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAllNotifications_WithFailedResult_ReturnsErrorResponse()
    {
        // Arrange
        var pageIndex = 0;
        var pageSize = 10;
        var cancellationToken = CancellationToken.None;

        var result = Result<GetAllNotificationsWithUnreadCountResponse>.Error("Failed to retrieve notifications");

        _mockSender.Setup(x => x.Send(It.IsAny<GetAllNotificationsQuery>(), cancellationToken))
            .ReturnsAsync(result);

        var errorResult = new BadRequestObjectResult("Failed to retrieve notifications");
        _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetAllNotificationsWithUnreadCountResponse>>(r => !r.IsSuccess)))
            .Returns(errorResult);

        // Act
        var actionResult = await _controller.GetAllNotifications(pageIndex, pageSize, cancellationToken);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult);
        _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetAllNotificationsWithUnreadCountResponse>>(r => !r.IsSuccess)), Times.Once);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 20)]
    [InlineData(2, 50)]
    public async Task GetAllNotifications_WithDifferentPaginationParameters_CallsWithCorrectParameters(int pageIndex, int pageSize)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var paginatedList = new PaginatedList<GetAllNotificationsQueryResponse>(new List<GetAllNotificationsQueryResponse>(), 0, pageIndex, pageSize);
        var queryResponse = new GetAllNotificationsWithUnreadCountResponse(paginatedList, 0);
        var result = Result.Success(queryResponse);

        _mockSender.Setup(x => x.Send(It.IsAny<GetAllNotificationsQuery>(), cancellationToken))
            .ReturnsAsync(result);

        // Act
        await _controller.GetAllNotifications(pageIndex, pageSize, cancellationToken);

        // Assert
        _mockSender.Verify(x => x.Send(It.Is<GetAllNotificationsQuery>(q => 
            q.PageIndex == pageIndex && 
            q.PageSize == pageSize), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationsAsRead_WithSuccessfulResult_ReturnsOkResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var commandResponse = new MarkAllNotificationsAsReadCommandResponse(true);
        var result = Result.Success(commandResponse);

        _mockSender.Setup(x => x.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), cancellationToken))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.MarkNotificationsAsRead(cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkResult>(actionResult);
        _mockSender.Verify(x => x.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationsAsRead_WithFailedResult_ReturnsErrorResponse()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var result = Result<MarkAllNotificationsAsReadCommandResponse>.Error("Failed to mark notifications as read");

        _mockSender.Setup(x => x.Send(It.IsAny<MarkAllNotificationsAsReadCommand>(), cancellationToken))
            .ReturnsAsync(result);

        var errorResult = new BadRequestObjectResult("Failed to mark notifications as read");
        _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<MarkAllNotificationsAsReadCommandResponse>>(r => !r.IsSuccess)))
            .Returns(errorResult);

        // Act
        var actionResult = await _controller.MarkNotificationsAsRead(cancellationToken);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult);
        _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<MarkAllNotificationsAsReadCommandResponse>>(r => !r.IsSuccess)), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var commandResponse = new MarkNotificationAsReadCommandResponse(true);
        var result = Result.Success(commandResponse);

        _mockSender.Setup(x => x.Send(It.Is<MarkNotificationAsReadCommand>(c => c.NotificationId == notificationId), cancellationToken))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.MarkNotificationAsRead(notificationId, cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(commandResponse, okResult.Value);
        _mockSender.Verify(x => x.Send(It.Is<MarkNotificationAsReadCommand>(c => c.NotificationId == notificationId), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WithFailedResult_ReturnsErrorResponse()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var result = Result<MarkNotificationAsReadCommandResponse>.Error("Notification not found");

        _mockSender.Setup(x => x.Send(It.IsAny<MarkNotificationAsReadCommand>(), cancellationToken))
            .ReturnsAsync(result);

        var errorResult = new NotFoundObjectResult("Notification not found");
        _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<MarkNotificationAsReadCommandResponse>>(r => !r.IsSuccess)))
            .Returns(errorResult);

        // Act
        var actionResult = await _controller.MarkNotificationAsRead(notificationId, cancellationToken);

        // Assert
        Assert.IsType<NotFoundObjectResult>(actionResult);
        _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<MarkNotificationAsReadCommandResponse>>(r => !r.IsSuccess)), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WithEmptyGuid_ReturnsErrorResponse()
    {
        // Arrange
        var notificationId = Guid.Empty;
        var cancellationToken = CancellationToken.None;
        var result = Result<MarkNotificationAsReadCommandResponse>.Invalid(new ValidationError("Invalid notification ID"));

        _mockSender.Setup(x => x.Send(It.IsAny<MarkNotificationAsReadCommand>(), cancellationToken))
            .ReturnsAsync(result);

        var errorResult = new BadRequestObjectResult("Invalid notification ID");
        _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<MarkNotificationAsReadCommandResponse>>(r => !r.IsSuccess)))
            .Returns(errorResult);

        // Act
        var actionResult = await _controller.MarkNotificationAsRead(notificationId, cancellationToken);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult);
        _mockSender.Verify(x => x.Send(It.Is<MarkNotificationAsReadCommand>(c => c.NotificationId == notificationId), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAllNotifications_WithDefaultParameters_UsesCorrectDefaults()
    {
        // Arrange
        var paginatedList = new PaginatedList<GetAllNotificationsQueryResponse>(new List<GetAllNotificationsQueryResponse>(), 0, 0, 10);
        var queryResponse = new GetAllNotificationsWithUnreadCountResponse(paginatedList, 0);
        var result = Result.Success(queryResponse);

        _mockSender.Setup(x => x.Send(It.IsAny<GetAllNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.GetAllNotifications(); // Using default parameters

        // Assert
        _mockSender.Verify(x => x.Send(It.Is<GetAllNotificationsQuery>(q => 
            q.PageIndex == 0 && 
            q.PageSize == 10), It.IsAny<CancellationToken>()), Times.Once);
    }
}

