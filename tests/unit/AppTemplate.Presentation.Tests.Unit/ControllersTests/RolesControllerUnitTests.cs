using System.Security.Claims;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.Roles.Commands.Create;
using AppTemplate.Application.Features.Roles.Commands.Delete;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;
using AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;
using AppTemplate.Application.Features.Roles.Queries.GetAllRoles;
using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Presentation.Controllers.Api.v1;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AppTemplate.Presentation.Tests.Unit.ControllersTests;

public class RolesControllerUnitTests
{
  private readonly Mock<ISender> _mockSender;
  private readonly Mock<IErrorHandlingService> _mockErrorHandlingService;
  private readonly RolesController _controller;

  public RolesControllerUnitTests()
  {
    _mockSender = new Mock<ISender>();
    _mockErrorHandlingService = new Mock<IErrorHandlingService>();

    _controller = new RolesController(
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

  #region GetAllRoles Tests

  [Fact]
  public async Task GetAllRoles_WithSuccessfulResult_ReturnsOkResult()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var rolesList = new List<GetAllRolesQueryResponse>
        {
            new(Guid.NewGuid().ToString(), "Admin", "Administrator", false),
            new(Guid.NewGuid().ToString(), "User", "Standard User", true)
        };

    var paginatedList = new PaginatedList<GetAllRolesQueryResponse>(rolesList, 2, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.Is<GetAllRolesQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetAllRoles(pageIndex, pageSize, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedList = Assert.IsAssignableFrom<PaginatedList<GetAllRolesQueryResponse>>(okResult.Value);
    Assert.Equal(2, returnedList.TotalCount);
    _mockSender.Verify(x => x.Send(It.IsAny<GetAllRolesQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetAllRoles_WithFailedResult_ReturnsErrorResponse()
  {
    // Arrange
    var pageIndex = 0;
    var pageSize = 10;
    var cancellationToken = CancellationToken.None;

    var result = Result<PaginatedList<GetAllRolesQueryResponse>>.Error("Failed to retrieve roles");

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllRolesQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Failed to retrieve roles");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllRolesQueryResponse>>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetAllRoles(pageIndex, pageSize, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<PaginatedList<GetAllRolesQueryResponse>>>(r => !r.IsSuccess)), Times.Once);
  }

  [Theory]
  [InlineData(0, 5)]
  [InlineData(1, 20)]
  [InlineData(2, 50)]
  public async Task GetAllRoles_WithDifferentPaginationParameters_CallsWithCorrectParameters(int pageIndex, int pageSize)
  {
    // Arrange
    var cancellationToken = CancellationToken.None;
    var paginatedList = new PaginatedList<GetAllRolesQueryResponse>(new List<GetAllRolesQueryResponse>(), 0, pageIndex, pageSize);
    var result = Result.Success(paginatedList);

    _mockSender.Setup(x => x.Send(It.IsAny<GetAllRolesQuery>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    await _controller.GetAllRoles(pageIndex, pageSize, cancellationToken);

    // Assert
    _mockSender.Verify(x => x.Send(It.Is<GetAllRolesQuery>(q =>
        q.PageIndex == pageIndex &&
        q.PageSize == pageSize), cancellationToken), Times.Once);
  }

  #endregion

  #region GetRoleById Tests

  [Fact]
  public async Task GetRoleById_WithValidId_ReturnsOkResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var roleResponse = new GetRoleByIdQueryResponse(
        roleId,
        "Admin",
        "Administrator",
        false);

    var result = Result.Success(roleResponse);

    _mockSender.Setup(x => x.Send(It.Is<GetRoleByIdQuery>(q => q.RoleId == roleId), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.GetRoleById(roleId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedRole = Assert.IsAssignableFrom<GetRoleByIdQueryResponse>(okResult.Value);
    Assert.Equal(roleId, returnedRole.Id);
    Assert.Equal("Admin", returnedRole.Name);
    _mockSender.Verify(x => x.Send(It.IsAny<GetRoleByIdQuery>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task GetRoleById_WithNonExistentId_ReturnsNotFoundResponse()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var result = Result<GetRoleByIdQueryResponse>.NotFound("Role not found");

    _mockSender.Setup(x => x.Send(It.IsAny<GetRoleByIdQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("Role not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetRoleByIdQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetRoleById(roleId, cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<GetRoleByIdQueryResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task GetRoleById_WithEmptyGuid_ReturnsValidationError()
  {
    // Arrange
    var roleId = Guid.Empty;
    var cancellationToken = CancellationToken.None;

    var result = Result<GetRoleByIdQueryResponse>.Invalid(new ValidationError("Invalid role ID"));

    _mockSender.Setup(x => x.Send(It.IsAny<GetRoleByIdQuery>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new BadRequestObjectResult("Invalid role ID");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<GetRoleByIdQueryResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.GetRoleById(roleId, cancellationToken);

    // Assert
    Assert.IsType<BadRequestObjectResult>(actionResult);
    _mockSender.Verify(x => x.Send(It.Is<GetRoleByIdQuery>(q => q.RoleId == roleId), cancellationToken), Times.Once);
  }

  #endregion

  #region CreateRole Tests

  [Fact]
  public async Task CreateRole_WithValidRequest_ReturnsOkResult()
  {
    // Arrange
    var request = new RolesController.CreateRoleRequest("NewRole", "New Role Display Name");
    var cancellationToken = CancellationToken.None;

    var commandResponse = new CreateRoleCommandResponse(Guid.NewGuid(), "NewRole", "New Role Display Name");
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.Is<CreateRoleCommand>(c =>
        c.Name == request.Name &&
        c.DisplayName == request.DisplayName), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.CreateRole(request, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<CreateRoleCommandResponse>(okResult.Value);
    Assert.Equal("NewRole", returnedResponse.Name);
    Assert.Equal("New Role Display Name", returnedResponse.DisplayName);
    _mockSender.Verify(x => x.Send(It.IsAny<CreateRoleCommand>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task CreateRole_WithDuplicateName_ReturnsConflictResponse()
  {
    // Arrange
    var request = new RolesController.CreateRoleRequest("ExistingRole", "Existing Role");
    var cancellationToken = CancellationToken.None;

    var result = Result<CreateRoleCommandResponse>.Error("Role with this name already exists");

    _mockSender.Setup(x => x.Send(It.IsAny<CreateRoleCommand>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new ConflictObjectResult("Role with this name already exists");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<CreateRoleCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.CreateRole(request, cancellationToken);

    // Assert
    Assert.IsType<ConflictObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<CreateRoleCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region UpdateRolePermissions Tests

  [Fact]
  public async Task UpdateRolePermissions_WithValidRequest_ReturnsOkResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var request = new RolesController.UpdateRolePermissionsRequest(permissionId, Operation.Add);
    var cancellationToken = CancellationToken.None;

    var commandResponse = new UpdateRolePermissionsCommandResponse(roleId, permissionId);
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.Is<UpdateRolePermissionsCommand>(c =>
        c.RoleId == roleId &&
        c.PermissionId == permissionId &&
        c.Operation == Operation.Add), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.UpdateRolePermissions(request, roleId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<UpdateRolePermissionsCommandResponse>(okResult.Value);
    Assert.Equal(roleId, returnedResponse.RoleId);
    Assert.Equal(permissionId, returnedResponse.PermissionId);
    _mockSender.Verify(x => x.Send(It.IsAny<UpdateRolePermissionsCommand>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task UpdateRolePermissions_WithRemoveOperation_ReturnsOkResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var permissionId = Guid.NewGuid();
    var request = new RolesController.UpdateRolePermissionsRequest(permissionId, Operation.Remove);
    var cancellationToken = CancellationToken.None;

    var commandResponse = new UpdateRolePermissionsCommandResponse(roleId, permissionId);
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateRolePermissionsCommand>(), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.UpdateRolePermissions(request, roleId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<UpdateRolePermissionsCommandResponse>(okResult.Value);
    _mockSender.Verify(x => x.Send(It.Is<UpdateRolePermissionsCommand>(c => c.Operation == Operation.Remove), cancellationToken), Times.Once);
  }

  #endregion

  #region UpdateRoleName Tests

  [Fact]
  public async Task UpdateRoleName_WithValidRequest_ReturnsOkResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var request = new RolesController.UpdateRoleNameRequest("UpdatedRole", "Updated Role Display Name");
    var cancellationToken = CancellationToken.None;

    var commandResponse = new UpdateRoleNameCommandResponse(roleId, "UpdatedRole", "Updated Role Display Name");
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.Is<UpdateRoleNameCommand>(c =>
        c.RoleId == roleId &&
        c.Name == request.Name &&
        c.DisplayName == request.DisplayName), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.UpdateRoleName(request, roleId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<UpdateRoleNameCommandResponse>(okResult.Value);
    Assert.Equal(roleId, returnedResponse.Id);
    Assert.Equal("UpdatedRole", returnedResponse.Name);
    Assert.Equal("Updated Role Display Name", returnedResponse.DisplayName);
    _mockSender.Verify(x => x.Send(It.IsAny<UpdateRoleNameCommand>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task UpdateRoleName_WithInvalidRoleId_ReturnsNotFoundResponse()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var request = new RolesController.UpdateRoleNameRequest("UpdatedRole", "Updated Role Display Name");
    var cancellationToken = CancellationToken.None;

    var result = Result<UpdateRoleNameCommandResponse>.NotFound("Role not found");

    _mockSender.Setup(x => x.Send(It.IsAny<UpdateRoleNameCommand>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("Role not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<UpdateRoleNameCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.UpdateRoleName(request, roleId, cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<UpdateRoleNameCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion

  #region DeleteRole Tests

  [Fact]
  public async Task DeleteRole_WithValidId_ReturnsOkResult()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var commandResponse = new DeleteRoleCommandResponse(roleId, "deletedrole");
    var result = Result.Success(commandResponse);

    _mockSender.Setup(x => x.Send(It.Is<DeleteRoleCommand>(c => c.RoleId == roleId), cancellationToken))
        .ReturnsAsync(result);

    // Act
    var actionResult = await _controller.DeleteRole(roleId, cancellationToken);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(actionResult);
    var returnedResponse = Assert.IsAssignableFrom<DeleteRoleCommandResponse>(okResult.Value);
    Assert.Equal(roleId, returnedResponse.Id);
    Assert.Equal("deletedrole", returnedResponse.Name);
    _mockSender.Verify(x => x.Send(It.IsAny<DeleteRoleCommand>(), cancellationToken), Times.Once);
  }

  [Fact]
  public async Task DeleteRole_WithNonExistentId_ReturnsNotFoundResponse()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var result = Result<DeleteRoleCommandResponse>.NotFound("Role not found");

    _mockSender.Setup(x => x.Send(It.IsAny<DeleteRoleCommand>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new NotFoundObjectResult("Role not found");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<DeleteRoleCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.DeleteRole(roleId, cancellationToken);

    // Assert
    Assert.IsType<NotFoundObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<DeleteRoleCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  [Fact]
  public async Task DeleteRole_WithRoleInUse_ReturnsConflictResponse()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var cancellationToken = CancellationToken.None;

    var result = Result<DeleteRoleCommandResponse>.Error("Cannot delete role that is assigned to users");

    _mockSender.Setup(x => x.Send(It.IsAny<DeleteRoleCommand>(), cancellationToken))
        .ReturnsAsync(result);

    var errorResult = new ConflictObjectResult("Cannot delete role that is assigned to users");
    _mockErrorHandlingService.Setup(x => x.HandleErrorResponse(It.Is<Result<DeleteRoleCommandResponse>>(r => !r.IsSuccess)))
        .Returns(errorResult);

    // Act
    var actionResult = await _controller.DeleteRole(roleId, cancellationToken);

    // Assert
    Assert.IsType<ConflictObjectResult>(actionResult);
    _mockErrorHandlingService.Verify(x => x.HandleErrorResponse(It.Is<Result<DeleteRoleCommandResponse>>(r => !r.IsSuccess)), Times.Once);
  }

  #endregion
}
