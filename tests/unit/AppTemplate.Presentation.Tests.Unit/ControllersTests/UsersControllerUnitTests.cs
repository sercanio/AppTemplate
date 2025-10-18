using System.Collections.ObjectModel;
using System.Security.Claims;
using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;
using AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Features.AppUsers.Queries.GetUser;
using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Presentation.Controllers.Api.v1;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AppTemplate.Presentation.Tests.Unit.ControllersTests;

public class UsersControllerUnitTests
{
  private readonly Mock<ISender> _mockSender;
  private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
  private readonly UsersController _controller;

  public UsersControllerUnitTests()
  {
    _mockSender = new Mock<ISender>();
    _mockErrorHandlingService = new Mock<IErrorHandlingService>();

    _controller = new UsersController(
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

  #region GetAllUsers Tests

  [Fact]
  public async Task GetAllUsers_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var rolesList = new Collection<LoggedInUserRolesDto>
        {
            new(Guid.NewGuid(), "Admin", "Administrator"),
            new(Guid.NewGuid(), "User", "Standard User")
        };

    var usersList = new List<GetAllUsersQueryResponse>
        {
            new(Guid.NewGuid(), "user1", true, DateTime.UtcNow.AddDays(-30), rolesList),
            new(Guid.NewGuid(), "user2", false, DateTime.UtcNow.AddDays(-15), new Collection<LoggedInUserRolesDto>())
        };

    var paginatedList = new PaginatedList<GetAllUsersQueryResponse>(usersList, usersList.Count, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.Is<GetAllUsersQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllUsers(pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<PaginatedList<GetAllUsersQueryResponse>>(okResult.Value);
    Assert.Equal(2, returnedList.TotalCount);
    Assert.True(returnedList.Items.First().EmailConfirmed);
    Assert.False(returnedList.Items.Last().EmailConfirmed);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllUsersQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllUsers_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var result = Result<PaginatedList<GetAllUsersQueryResponse>>.Error("Failed to retrieve users");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Failed to retrieve users");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllUsersQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllUsers(pageIndex, pageSize, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllUsersQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  [Theory]
  [InlineData(0, 5)]
  [InlineData(1, 20)]
  [InlineData(2, 50)]
  public async Task GetAllUsers_WithDifferentPaginationParameters_CallsWithCorrectParameters(int pageIndex, int pageSize)
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var paginatedList = new PaginatedList<GetAllUsersQueryResponse>(new List<GetAllUsersQueryResponse>(), 0, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllUsers(pageIndex, pageSize, cancellationToken);

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllUsersQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize), cancellationToken), Times.Once);
  }

  #endregion

  #region GetAllUsersDynamic Tests

  [Fact]
  public async Task GetAllUsersDynamic_WithValidDynamicQuery_ReturnsOkResult()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;
    var dynamicQuery = new DynamicQuery
    {
      Filter = new Filter { Field = "Email", Operator = "contains", Value = "test" },
      Sort = new List<Sort> { new() { Field = "UserName", Dir = "asc" } }
    };

    var rolesList = new Collection<LoggedInUserRolesDto>
        {
            new(Guid.NewGuid(), "User", "Standard User")
        };

    var usersList = new List<GetAllUsersDynamicQueryResponse>
        {
            new(Guid.NewGuid(), "testuser1", rolesList),
            new(Guid.NewGuid(), "testuser2", new Collection<LoggedInUserRolesDto>())
        };

    var paginatedList = new PaginatedList<GetAllUsersDynamicQueryResponse>(usersList, usersList.Count, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.Is<GetAllUsersDynamicQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize &&
        q.DynamicQuery == dynamicQuery), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllUsersDynamic(dynamicQuery, pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<PaginatedList<GetAllUsersDynamicQueryResponse>>(okResult.Value);
    Assert.Equal(2, returnedList.TotalCount);
    Assert.Equal("testuser1", returnedList.Items.First().UserName);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllUsersDynamicQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllUsersDynamic_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var dynamicQuery = new DynamicQuery();
    var cancellationToken = CancellationToken.None;

    var result = Result<PaginatedList<GetAllUsersDynamicQueryResponse>>.Error("Dynamic query failed");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersDynamicQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Dynamic query failed");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllUsersDynamic(dynamicQuery, 0, 10, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task GetAllUsersDynamic_WithEmptyDynamicQuery_ReturnsOkResult()
  {
    // Arrange
    var dynamicQuery = new DynamicQuery();
    var cancellationToken = CancellationToken.None;

    var paginatedList = new PaginatedList<GetAllUsersDynamicQueryResponse>(new List<GetAllUsersDynamicQueryResponse>(), 0, 0, 10);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersDynamicQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllUsersDynamic(dynamicQuery, 0, 10, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<PaginatedList<GetAllUsersDynamicQueryResponse>>(okResult.Value);
    Assert.Empty(returnedList.Items);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllUsersDynamicQuery>(), cancellationToken), Times.Once);
  }

  #endregion

  #region GetUserById Tests

  [Fact]
  public async Task GetUserById_WithValidId_ReturnsOkResult()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var rolesList = new Collection<GetRoleByIdQueryResponse>
        {
            new(Guid.NewGuid(), "Admin", "Administrator", false)
        };

    var userResponse = new GetUserQueryResponse(userId, "testuser", rolesList);

    var result = Result.Success(userResponse);

    _mockSender.Setup(x => x.Send(It.Is<GetUserQuery>(q => q.UserId == userId), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetUserById(userId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedUser = Assert.IsAssignableFrom<GetUserQueryResponse>(okResult.Value);
    Assert.Equal(userId, returnedUser.Id);
    Assert.Equal("testuser", returnedUser.UserName);
    Assert.Single(returnedUser.Roles);
    _mockSender.Verify(x => x.Send(It.IsAny<GetUserQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetUserById_WithNonExistentId_ReturnsNotFoundResponse()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var result = Result<GetUserQueryResponse>.NotFound("User not found");

    _mockSender.Setup(x => x.Send(It.IsAny<GetUserQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetUserQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetUserById(userId, cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetUserQueryResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task GetUserById_WithEmptyGuid_ReturnsValidationError()
  {
    // Arrange
    var userId = Guid.Empty;
    var cancellationToken = CancellationToken.None;

    var result = Result<GetUserQueryResponse>.Invalid(new ValidationError("Invalid user ID"));

    _mockSender.Setup(x => x.Send(It.IsAny<GetUserQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Invalid user ID");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetUserQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetUserById(userId, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockSender.Verify(x => x.Send(It.Is<GetUserQuery>(q => q.UserId == userId), cancellationToken), Times.Once);
  }

  #endregion

  #region GetAllUsersByRoleId Tests

  [Fact]
  public async Task GetAllUsersByRoleId_WithValidRoleId_ReturnsOkResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var rolesList = new Collection<LoggedInUserRolesDto>
        {
            new(roleId, "Admin", "Administrator")
        };

    var usersList = new List<GetAllUsersByRoleIdQueryResponse>
        {
            new(Guid.NewGuid(), "admin1", rolesList),
            new(Guid.NewGuid(), "admin2", rolesList)
        };

    var paginatedList = new PaginatedList<GetAllUsersByRoleIdQueryResponse>(usersList, usersList.Count, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.Is<GetAllUsersByRoleIdQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize &&
        q.RoleId == roleId), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllUsersByRoleId(roleId, pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<PaginatedList<GetAllUsersByRoleIdQueryResponse>>(okResult.Value);
    Assert.Equal(2, returnedList.TotalCount);
    Assert.Equal("admin1", returnedList.Items.First().UserName);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllUsersByRoleIdQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllUsersByRoleId_WithNonExistentRoleId_ReturnsEmptyResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var paginatedList = new PaginatedList<GetAllUsersByRoleIdQueryResponse>(new List<GetAllUsersByRoleIdQueryResponse>(), 0, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersByRoleIdQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllUsersByRoleId(roleId, pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<PaginatedList<GetAllUsersByRoleIdQueryResponse>>(okResult.Value);
    Assert.Empty(returnedList.Items);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllUsersByRoleIdQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllUsersByRoleId_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var result = Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>.Error("Failed to retrieve users by role");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersByRoleIdQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Failed to retrieve users by role");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllUsersByRoleId(roleId, 0, 10, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region UpdateUserRoles Tests

  [Fact]
  public async Task UpdateUserRoles_WithAddOperation_ReturnsOkResult()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var request = new UpdateUserRolesRequest(Operation.Add, roleId);
    var cancellationToken = CancellationToken.None;

    var commandResponse = new UpdateUserRolesCommandResponse(roleId, userId);
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.Is<UpdateUserRolesCommand>(c =>
        c.UserId == userId &&
        c.Operation == Operation.Add &&
        c.RoleId == roleId), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.UpdateUserRoles(request, userId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<UpdateUserRolesCommandResponse>(okResult.Value);
    Assert.Equal(userId, returnedResponse.UserId);
    Assert.Equal(roleId, returnedResponse.RoleId);
    _mockSender.Verify(x => x.Send(It.IsAny<UpdateUserRolesCommand>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task UpdateUserRoles_WithRemoveOperation_ReturnsOkResult()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var request = new UpdateUserRolesRequest(Operation.Remove, roleId);
    var cancellationToken = CancellationToken.None;

    var commandResponse = new UpdateUserRolesCommandResponse(roleId, userId);
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateUserRolesCommand>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.UpdateUserRoles(request, userId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<UpdateUserRolesCommandResponse>(okResult.Value);
    _mockSender.Verify(x => x.Send(It.Is<UpdateUserRolesCommand>(c => c.Operation == Operation.Remove), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task UpdateUserRoles_WithInvalidUserId_ReturnsNotFoundResponse()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var request = new UpdateUserRolesRequest(Operation.Add, roleId);
    var cancellationToken = CancellationToken.None;

    var result = Result<UpdateUserRolesCommandResponse>.NotFound("User not found");

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateUserRolesCommand>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("User not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<UpdateUserRolesCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.UpdateUserRoles(request, userId, cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<UpdateUserRolesCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task UpdateUserRoles_WithInvalidRoleId_ReturnsErrorResponse()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var request = new UpdateUserRolesRequest(Operation.Add, roleId);
    var cancellationToken = CancellationToken.None;

    var result = Result<UpdateUserRolesCommandResponse>.Error("Role not found");

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateUserRolesCommand>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Role not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<UpdateUserRolesCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.UpdateUserRoles(request, userId, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<UpdateUserRolesCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region Default Parameters Tests

  [Fact]
  public async Task GetAllUsers_WithDefaultParameters_UsesCorrectDefaults()
  {
    // Arrange
    var paginatedList = new PaginatedList<GetAllUsersQueryResponse>(new List<GetAllUsersQueryResponse>(), 0, 0, 10);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllUsers(); // Using default parameters

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllUsersQuery>(q =>
        q.PageIndex == 0 &&
        q.PageSize == 10), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAllUsersByRoleId_WithDefaultParameters_UsesCorrectDefaults()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var paginatedList = new PaginatedList<GetAllUsersByRoleIdQueryResponse>(new List<GetAllUsersByRoleIdQueryResponse>(), 0, 0, 10);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllUsersByRoleIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllUsersByRoleId(roleId); // Using default parameters

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllUsersByRoleIdQuery>(q =>
        q.PageIndex == 0 &&
        q.PageSize == 10 &&
        q.RoleId == roleId), It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion
}
