using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Web.Controllers;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AppTemplate.Web.Tests.Unit.ControllerTests;

public class PermissionsControllerUnitTests
{
  private readonly Mock<ISender> _mockSender;
  private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
  private readonly PermissionsController _controller;

  public PermissionsControllerUnitTests()
  {
    _mockSender = new Mock<ISender>();
    _mockErrorHandlingService = new Mock<IErrorHandlingService>();

    _controller = new PermissionsController(
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
  public async Task GetAllPermissions_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var permissionsList = new List<GetAllPermissionsQueryResponse>
        {
            new(Guid.NewGuid(), "users:read", "Read Users"),
            new(Guid.NewGuid(), "users:create", "Create Users"),
            new(Guid.NewGuid(), "permissions:read", "Read Permissions")
        };

    var mockPaginatedList = new Mock<IPaginatedList<GetAllPermissionsQueryResponse>>();
    mockPaginatedList.Setup(x => x.Items).Returns(permissionsList);
    mockPaginatedList.Setup(x => x.TotalCount).Returns(3);
    mockPaginatedList.Setup(x => x.PageIndex).Returns(pageIndex);
    mockPaginatedList.Setup(x => x.PageSize).Returns(pageSize);
    mockPaginatedList.Setup(x => x.HasPreviousPage).Returns(false);
    mockPaginatedList.Setup(x => x.HasNextPage).Returns(false);

    var result = Result.Success(mockPaginatedList.Object);

    _mockSender.Setup(x => x.Send(It.Is<GetAllPermissionsQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    Assert.Equal(mockPaginatedList.Object, okResult.Value);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllPermissions_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var result = Result<IPaginatedList<GetAllPermissionsQueryResponse>>.Error("Failed to retrieve permissions");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Failed to retrieve permissions");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<IPaginatedList<GetAllPermissionsQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<IPaginatedList<GetAllPermissionsQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  [Theory]
  [InlineData(0, 5)]
  [InlineData(1, 20)]
  [InlineData(2, 50)]
  [InlineData(0, 100)]
  public async Task GetAllPermissions_WithDifferentPaginationParameters_CallsWithCorrectParameters(int pageIndex, int pageSize)
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var mockPaginatedList = new Mock<IPaginatedList<GetAllPermissionsQueryResponse>>();
    mockPaginatedList.Setup(x => x.Items).Returns(new List<GetAllPermissionsQueryResponse>());
    mockPaginatedList.Setup(x => x.TotalCount).Returns(0);
    mockPaginatedList.Setup(x => x.PageIndex).Returns(pageIndex);
    mockPaginatedList.Setup(x => x.PageSize).Returns(pageSize);

    var result = Result.Success(mockPaginatedList.Object);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllPermissionsQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllPermissions_WithDefaultParameters_UsesCorrectDefaults()
  {
    // Arrange
    var mockPaginatedList = new Mock<IPaginatedList<GetAllPermissionsQueryResponse>>();
    mockPaginatedList.Setup(x => x.Items).Returns(new List<GetAllPermissionsQueryResponse>());
    mockPaginatedList.Setup(x => x.TotalCount).Returns(0);
    mockPaginatedList.Setup(x => x.PageIndex).Returns(0);
    mockPaginatedList.Setup(x => x.PageSize).Returns(10);

    var result = Result.Success(mockPaginatedList.Object);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllPermissions(); // Using default parameters

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllPermissionsQuery>(q =>
        q.PageIndex == 0 &&
        q.PageSize == 10), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAllPermissions_WithNotFoundResult_ReturnsNotFoundResponse()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var result = Result<IPaginatedList<GetAllPermissionsQueryResponse>>.NotFound("No permissions found");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("No permissions found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<IPaginatedList<GetAllPermissionsQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<IPaginatedList<GetAllPermissionsQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task GetAllPermissions_WithInvalidParameters_ReturnsValidationErrorResponse()
  {
    // Arrange
    var pageIndex = -1; // Invalid page index
    var pageSize = 0;   // Invalid page size
    var cancellationToken = CancellationToken.None;

    var validationError = new ValidationError("Invalid pagination parameters");
    var result = Result<IPaginatedList<GetAllPermissionsQueryResponse>>.Invalid(validationError);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Invalid pagination parameters");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<IPaginatedList<GetAllPermissionsQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<IPaginatedList<GetAllPermissionsQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task GetAllPermissions_WithEmptyResult_ReturnsOkWithEmptyList()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var mockPaginatedList = new Mock<IPaginatedList<GetAllPermissionsQueryResponse>>();
    mockPaginatedList.Setup(x => x.Items).Returns(new List<GetAllPermissionsQueryResponse>());
    mockPaginatedList.Setup(x => x.TotalCount).Returns(0);
    mockPaginatedList.Setup(x => x.PageIndex).Returns(pageIndex);
    mockPaginatedList.Setup(x => x.PageSize).Returns(pageSize);
    mockPaginatedList.Setup(x => x.HasPreviousPage).Returns(false);
    mockPaginatedList.Setup(x => x.HasNextPage).Returns(false);

    var result = Result.Success(mockPaginatedList.Object);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<IPaginatedList<GetAllPermissionsQueryResponse>>(okResult.Value);
    Assert.Empty(returnedList.Items);
    Assert.Equal(0, returnedList.TotalCount);
  }

  [Fact]
  public async Task GetAllPermissions_VerifyQueryParametersArePassedCorrectly()
  {
    // Arrange
    var pageIndex = 3;
    var pageSize = 25;
    var cancellationToken = new CancellationToken();

    var mockPaginatedList = new Mock<IPaginatedList<GetAllPermissionsQueryResponse>>();
    var result = Result.Success(mockPaginatedList.Object);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllPermissionsQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllPermissions(pageIndex, pageSize, cancellationToken);

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllPermissionsQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize),
        cancellationToken), Times.Once);
  }
}
