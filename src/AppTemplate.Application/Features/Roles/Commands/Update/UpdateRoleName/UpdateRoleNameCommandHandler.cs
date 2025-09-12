using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

public sealed class UpdateRoleNameCommandHandler(
    IRolesRepository rolesRepository,
    IAppUsersRepository usersRepository,
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : ICommandHandler<UpdateRoleNameCommand, UpdateRoleNameCommandResponse>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IAppUsersRepository _usersRepository = usersRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<UpdateRoleNameCommandResponse>> Handle(UpdateRoleNameCommand request, CancellationToken cancellationToken)
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

        role.ChangeName(request.Name, updatedById: user.Id);
        role.ChangeDisplayName(request.DisplayName, updatedById: user.Id);

        _rolesRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await InvalidateRoleCacheAsync(role.Id, cancellationToken);

        return Result.Success(new UpdateRoleNameCommandResponse(role.Name.Value));
    }

    private async Task InvalidateRoleCacheAsync(Guid roleId, CancellationToken cancellationToken)
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
