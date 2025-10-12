using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Enums;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdatePermissions;

public sealed class UpdateRolePermissionsCommandHandler(
    IRolesRepository rolesRepository,
    IPermissionsRepository permissionRepository,
    IAppUsersRepository usersRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<UpdateRolePermissionsCommand, UpdateRolePermissionsCommandResponse>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IPermissionsRepository _permissionRepository = permissionRepository;
    private readonly IAppUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<UpdateRolePermissionsCommandResponse>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
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

        var roleResult = await _rolesRepository.GetRoleByIdWithPermissionsAsync(request.RoleId, cancellationToken);
        if (!roleResult.IsSuccess || roleResult.Value is null)
        {
            return Result.NotFound();
        }
        var role = roleResult.Value;

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
            var result = await _usersRepository.GetAllUsersByRoleIdWithIdentityAndRolesAsync(
                roleId,
                pageIndex,
                batchSize,
                cancellationToken);

            usersBatch = result.Value ?? new PaginatedList<AppUser>(new List<AppUser>(), 0, pageIndex, batchSize);

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
