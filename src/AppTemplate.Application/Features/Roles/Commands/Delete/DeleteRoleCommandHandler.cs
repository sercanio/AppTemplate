using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Roles.Commands.Delete;

public sealed class DeleteRoleCommandHandler(
    IRolesRepository rolesRepository,
    IAppUsersRepository usersRepository,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    ICacheService cacheService) : ICommandHandler<DeleteRoleCommand, DeleteRoleCommandResponse>
{
  private readonly IRolesRepository _rolesRepository = rolesRepository;
  private readonly IAppUsersRepository _usersRepository = usersRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result<DeleteRoleCommandResponse>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
  {
    var roleResult = await _rolesRepository.GetRoleByIdWithPermissionsAsync(request.RoleId, cancellationToken);
    if (!roleResult.IsSuccess || roleResult.Value is null)
    {
      return Result.NotFound(RoleErrors.NotFound.Name);
    }
    var role = roleResult.Value;

    var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(identityId))
    {
      return Result.Error("Current user not authenticated.");
    }

    var userResult = await _usersRepository.GetUserByIdentityIdWithIdentityAndRolesAsync(identityId, cancellationToken);
    if (!userResult.IsSuccess || userResult.Value is null)
    {
      return Result.NotFound("User not found.");
    }
    var user = userResult.Value;

    _ = Role.Delete(role, deletedById: user.Id);
    _rolesRepository.Update(role);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await _cacheService.RemoveAsync($"roles-{role.Id}", cancellationToken);

    DeleteRoleCommandResponse response = new(role.Id, role.Name.Value);
    return Result.Success(response);
  }
}
