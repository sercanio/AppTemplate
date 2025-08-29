using AppTemplate.Application.Enums;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Myrtus.Clarity.Core.Application.Abstractions.Caching;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Data;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

public sealed class UpdateRolePermissionsCommandHandler(
    IRolesRepository rolesRepository,
    IPermissionsRepository permissionRepository,
    IAppUsersService userService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<UpdateRolePermissionsCommand, UpdateRolePermissionsCommandResponse>
{
  private readonly IRolesRepository _rolesRepository = rolesRepository;
  private readonly IPermissionsRepository _permissionRepository = permissionRepository;
  private readonly IAppUsersService _userService = userService;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<Result<UpdateRolePermissionsCommandResponse>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
  {
    var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _userService.GetAsync(
        predicate: u => u.IdentityId == identityId,
        includeSoftDeleted: false,
        include: query => query
            .Include(u => u.IdentityUser),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    var role = await _rolesRepository.GetAsync(
        predicate: r => r.Id == request.RoleId,
       include: query => query
            .Include(r => r.Permissions),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (role is null)
    {
      return Result.NotFound();
    }

    var permission = role.Permissions.FirstOrDefault(p => p.Id == request.PermissionId);

    var permissionToAdd = await _permissionRepository.GetAsync(
        predicate: permission => permission.Id == request.PermissionId,
        cancellationToken: cancellationToken);

    if (request.Operation == Operation.Add && permission is null)
    {

      if (permissionToAdd is null)
      {
        return Result.NotFound($"Permission with ID {request.PermissionId} not found.");
      }

      role.AddPermission(permissionToAdd, updatedById: user.Id);
    }
    else if (request.Operation == Operation.Remove && permission is not null)
    {
      role.RemovePermission(permission, updatedById: user.Id);
    }
    else
    {
      return Result.Invalid();
    }



    _rolesRepository.Update(role);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await InvalidateCachesAsync(role.Id, cancellationToken);

    return Result.Success(new UpdateRolePermissionsCommandResponse(role.Id, request.PermissionId));
  }

  private async Task InvalidateCachesAsync(Guid roleId, CancellationToken cancellationToken)
  {
    await _cacheService.RemoveAsync($"roles-{roleId}", cancellationToken);

    const int batchSize = 1000;
    int pageIndex = 0;
    PaginatedList<AppUser> usersBatch;

    do
    {
      usersBatch = await _userService.GetAllAsync(
          index: pageIndex,
          size: batchSize,
          predicate: u => u.Roles.Any(r => r.Id == roleId),
          includeSoftDeleted: false,
          cancellationToken: cancellationToken);

      var tasks = usersBatch.Items.Select(async u =>
      {
        await _cacheService.RemoveAsync($"auth:roles-{u.IdentityId}", cancellationToken);
        await _cacheService.RemoveAsync($"auth:permissions-{u.IdentityId}", cancellationToken);
      });

      await Task.WhenAll(tasks);

      pageIndex++;
    } while (usersBatch.Items.Count > 0);
  }
}
