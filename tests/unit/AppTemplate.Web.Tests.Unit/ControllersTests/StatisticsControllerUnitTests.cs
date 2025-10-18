using System.Security.Claims;
using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;
using AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Web.Controllers.Api.v1;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AppTemplate.Web.Tests.Unit.ControllersTests;

public class StatisticsControllerUnitTests
{
  private readonly Mock<ISender> _mockSender;
  private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
  private readonly StatisticsController _controller;

  public StatisticsControllerUnitTests()
  {
    _mockSender = new Mock<ISender>();
    _mockErrorHandlingService = new Mock<IErrorHandlingService>();

    _controller = new StatisticsController(
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

  #region GetUsersCount Tests

  [Fact]
  public async Task GetUsersCount_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var usersCountResponse = new GetUsersCountQueryResponse(150);
    var result = Result.Success(usersCountResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetUsersCountQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetUsersCount(cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<GetUsersCountQueryResponse>(okResult.Value);
    Assert.Equal(150, returnedResponse.Count);
    _mockSender.Verify(x => x.Send(It.IsAny<GetUsersCountQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetUsersCount_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var result = Result<GetUsersCountQueryResponse>.Error("Failed to retrieve user count");

    _mockSender.Setup(x => x.Send(It.IsAny<GetUsersCountQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Failed to retrieve user count");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetUsersCountQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetUsersCount(cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetUsersCountQueryResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region GetUserRegistrationTrends Tests

  [Fact]
  public async Task GetUserRegistrationTrends_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var dailyRegistrations = new Dictionary<string, int>
        {
            { "2024-01-01", 10 },
            { "2024-01-02", 15 },
            { "2024-01-03", 8 }
        };
    var trendsResponse = new GetUserRegistrationTrendsQueryResponse(
        TotalUsersLastMonth: 120,
        TotalUsersThisMonth: 150,
        GrowthPercentage: 25,
        DailyRegistrations: dailyRegistrations);
    var result = Result.Success(trendsResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetUserRegistrationTrendsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetUserRegistrationTrends(cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<GetUserRegistrationTrendsQueryResponse>(okResult.Value);
    Assert.Equal(120, returnedResponse.TotalUsersLastMonth);
    Assert.Equal(150, returnedResponse.TotalUsersThisMonth);
    Assert.Equal(25, returnedResponse.GrowthPercentage);
    Assert.Equal(3, returnedResponse.DailyRegistrations.Count);
    _mockSender.Verify(x => x.Send(It.IsAny<GetUserRegistrationTrendsQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetUserRegistrationTrends_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var result = Result<GetUserRegistrationTrendsQueryResponse>.NotFound("No registration data found");

    _mockSender.Setup(x => x.Send(It.IsAny<GetUserRegistrationTrendsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("No registration data found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetUserRegistrationTrendsQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetUserRegistrationTrends(cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetUserRegistrationTrendsQueryResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region GetAuthenticationStatistics Tests

  [Fact]
  public async Task GetAuthenticationStatistics_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var authStatsResponse = new GetAuthenticationStatisticsQueryResponse(
        ActiveSessions: 50,
        SuccessfulLogins: 475,
        FailedLogins: 25,
        TwoFactorEnabled: 30,
        TotalUsersWithAuthenticator: 25);
    var result = Result.Success(authStatsResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAuthenticationStatisticsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAuthenticationStatistics(cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<GetAuthenticationStatisticsQueryResponse>(okResult.Value);
    Assert.Equal(50, returnedResponse.ActiveSessions);
    Assert.Equal(475, returnedResponse.SuccessfulLogins);
    Assert.Equal(25, returnedResponse.FailedLogins);
    Assert.Equal(30, returnedResponse.TwoFactorEnabled);
    Assert.Equal(25, returnedResponse.TotalUsersWithAuthenticator);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAuthenticationStatisticsQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAuthenticationStatistics_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var result = Result<GetAuthenticationStatisticsQueryResponse>.Error("Authentication stats unavailable");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAuthenticationStatisticsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Authentication stats unavailable");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetAuthenticationStatisticsQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAuthenticationStatistics(cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetAuthenticationStatisticsQueryResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region GetRoleStatistics Tests

  [Fact]
  public async Task GetRoleStatistics_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var roleStatsResponse = new GetRoleStatisticsQueryResponse(
        TotalRoles: 10,
        TotalPermissions: 25,
        PermissionsPerRole: new Dictionary<string, int> { { "Admin", 15 }, { "User", 5 }, { "Manager", 10 } },
        UsersPerRole: new Dictionary<string, int> { { "Admin", 5 }, { "User", 95 }, { "Manager", 20 } },
        PermissionsByFeature: new Dictionary<string, int> { { "Users", 8 }, { "Roles", 6 }, { "Statistics", 4 }, { "Notifications", 7 } });
    var result = Result.Success(roleStatsResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetRoleStatisticsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetRoleStatistics(cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<GetRoleStatisticsQueryResponse>(okResult.Value);
    Assert.Equal(10, returnedResponse.TotalRoles);
    Assert.Equal(25, returnedResponse.TotalPermissions);
    Assert.Equal(3, returnedResponse.PermissionsPerRole.Count);
    Assert.Equal(3, returnedResponse.UsersPerRole.Count);
    Assert.Equal(4, returnedResponse.PermissionsByFeature.Count);

    // Verify specific dictionary values
    Assert.Equal(15, returnedResponse.PermissionsPerRole["Admin"]);
    Assert.Equal(5, returnedResponse.PermissionsPerRole["User"]);
    Assert.Equal(95, returnedResponse.UsersPerRole["User"]);
    Assert.Equal(8, returnedResponse.PermissionsByFeature["Users"]);

    _mockSender.Verify(x => x.Send(It.IsAny<GetRoleStatisticsQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetRoleStatistics_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var result = Result<GetRoleStatisticsQueryResponse>.Error("Role statistics unavailable");

    _mockSender.Setup(x => x.Send(It.IsAny<GetRoleStatisticsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Role statistics unavailable");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetRoleStatisticsQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetRoleStatistics(cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetRoleStatisticsQueryResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task GetRoleStatistics_WithEmptyDictionaries_ReturnsOkResult()
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var roleStatsResponse = new GetRoleStatisticsQueryResponse(
        TotalRoles: 0,
        TotalPermissions: 0,
        PermissionsPerRole: new Dictionary<string, int>(),
        UsersPerRole: new Dictionary<string, int>(),
        PermissionsByFeature: new Dictionary<string, int>());
    var result = Result.Success(roleStatsResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetRoleStatisticsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetRoleStatistics(cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<GetRoleStatisticsQueryResponse>(okResult.Value);
    Assert.Equal(0, returnedResponse.TotalRoles);
    Assert.Equal(0, returnedResponse.TotalPermissions);
    Assert.Empty(returnedResponse.PermissionsPerRole);
    Assert.Empty(returnedResponse.UsersPerRole);
    Assert.Empty(returnedResponse.PermissionsByFeature);
    _mockSender.Verify(x => x.Send(It.IsAny<GetRoleStatisticsQuery>(), cancellationToken), Times.Once);
  }

  #endregion

  #region Default CancellationToken Tests

  [Fact]
  public async Task GetUsersCount_WithDefaultCancellationToken_CallsCorrectly()
  {
    // Arrange
    var usersCountResponse = new GetUsersCountQueryResponse(100);
    var result = Result.Success(usersCountResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetUsersCountQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    // Act
    await _controller.GetUsersCount(); // Using default CancellationToken

    // Assert
    _mockSender.Verify(x => x.Send(It.IsAny<GetUsersCountQuery>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetUserRegistrationTrends_WithDefaultCancellationToken_CallsCorrectly()
  {
    // Arrange
    var trendsResponse = new GetUserRegistrationTrendsQueryResponse(0, 0, 0, new Dictionary<string, int>());
    var result = Result.Success(trendsResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<GetUserRegistrationTrendsQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    // Act
    await _controller.GetUserRegistrationTrends(); // Using default CancellationToken

    // Assert
    _mockSender.Verify(x => x.Send(It.IsAny<GetUserRegistrationTrendsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion
}
