using AppTemplate.Application.Enums;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AppTemplate.Core.Application.Abstractions.Caching;
using AppTemplate.Core.Application.Abstractions.Messaging;
using AppTemplate.Core.Domain.Abstractions;
using System.Security.Claims;

namespace AppTemplate.Application.Features.AppUsers.Commands.Update.UpdateUserRoles;

public sealed class UpdateUserRolesCommandHandler(
        IAppUsersRepository userRepository,
        IRolesService rolesService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IHttpContextAccessor httpContextAccessor) : ICommandHandler<UpdateUserRolesCommand, UpdateUserRolesCommandResponse>
{
  private readonly IAppUsersRepository _userRepository = userRepository;
  private readonly IRolesService _rolesService = rolesService;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<Result<UpdateUserRolesCommandResponse>> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetAsync(
        predicate: user => user.Id == request.UserId,
        include: query => query
            .Include(u => u.Roles)
            .Include(u => u.IdentityUser),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (user is null)
    {
      return Result.NotFound(AppUserErrors.NotFound.Name);
    }

    var role = await _rolesService.GetAsync(
        predicate: role => role.Id == request.RoleId,
        asNoTracking: false,
        cancellationToken: cancellationToken);

    if (role is null)
    {
      return Result.NotFound(RoleErrors.NotFound.Name);
    }

    string? userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdClaim))
    {
      return Result.Error("Current user not authenticated.");
    }

    var actorUser = await _userRepository.GetAsync(
        predicate: u => u.IdentityId == userIdClaim.ToString(),
        include: query => query
            .Include(u => u.Roles)
            .Include(u => u.IdentityUser),
        asNoTracking: false,
        cancellationToken: cancellationToken);

    // Track previous roles for moderation log
    //var previousRoles = user.Roles.Where(r => r.Name.Value != "Registered").Select(r => r.Name.Value).ToList();
    switch (request.Operation)
    {
      case Operation.Add:
        user.AddRole(role);
        break;
      case Operation.Remove:
        user.RemoveRole(role);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    _userRepository.Update(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await InvalidateUserCacheAsync(user, cancellationToken);

    return Result.Success(new UpdateUserRolesCommandResponse(role.Id, user.Id));
  }

  private async Task InvalidateUserCacheAsync(AppUser user, CancellationToken cancellationToken)
  {
    await _cacheService.RemoveAsync($"users-{user.Id}", cancellationToken);
    await _cacheService.RemoveAsync($"auth:roles-{user.IdentityId}", cancellationToken);
    await _cacheService.RemoveAsync($"auth:permissions-{user.IdentityId}", cancellationToken);
  }
}