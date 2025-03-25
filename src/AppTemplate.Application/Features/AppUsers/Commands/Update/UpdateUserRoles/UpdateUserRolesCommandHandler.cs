using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ardalis.Result;
using Myrtus.Clarity.Core.Application.Abstractions.Caching;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Roles;

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
            include: [
                u => u.Roles,
                u => u.IdentityUser
            ],
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return Result.NotFound(AppUserErrors.NotFound.Name);
        }

        var role = await _rolesService.GetAsync(
            predicate: role => role.Id == request.RoleId,
            cancellationToken: cancellationToken);

        if (role is null)
        {
            return Result.NotFound(RoleErrors.NotFound.Name);
        }

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

        // Get current user's Id from claims
        string? userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Result.Error("Current user not authenticated.");
        }

        var modifierUser = await _userRepository.GetAsync(
            predicate: user => user.IdentityUser.Id == userIdClaim,
            include: [
                u => u.Roles,
                u => u.IdentityUser
            ],
            cancellationToken: cancellationToken);

        if (modifierUser is null)
        {
            return Result.Error("Modifier user not found.");
        }
        user.UpdatedBy = modifierUser.IdentityUser?.Email ?? "System";

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
