using AppTemplate.Application.Enums;
using AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Core.Application.Abstractions.Caching;
using AppTemplate.Core.Domain.Abstractions;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Commands.UpdateUserRolesTests;

[Trait("Category", "Unit")]
public class UpdateUserRolesCommandHandlerUnitTests
{
  private readonly Mock<IAppUsersRepository> _userRepositoryMock = new();
  private readonly Mock<IRolesService> _rolesServiceMock = new();
  private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
  private readonly Mock<ICacheService> _cacheServiceMock = new();
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
  private readonly UpdateUserRolesCommandHandler _handler;

  public UpdateUserRolesCommandHandlerUnitTests()
  {
    _handler = new UpdateUserRolesCommandHandler(
        _userRepositoryMock.Object,
        _rolesServiceMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object,
        _httpContextAccessorMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenUserNotFound()
  {
    var command = new UpdateUserRolesCommand(Guid.NewGuid(), Operation.Add, Guid.NewGuid());

    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AppUser>.NotFound("User not found"));

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task Handle_ReturnsNotFound_WhenRoleNotFound()
  {
    var userId = Guid.NewGuid();
    var roleId = Guid.NewGuid();
    var command = new UpdateUserRolesCommand(userId, Operation.Add, roleId);

    var appUser = AppUser.Create();
    _userRepositoryMock
        .Setup(r => r.GetUserWithRolesAndIdentityByIdAsync(userId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(appUser));

    _rolesServiceMock
        .Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Role, bool>>>(),
            false,
            null,
            true,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((Role)null);

    var result = await _handler.Handle(command, default);

    Assert.Equal(ResultStatus.NotFound, result.Status);
  }
}