using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ardalis.Result;
using Myrtus.Clarity.Core.Application.Abstractions.Caching;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.AppUsers;

namespace AppTemplate.Application.Features.Roles.Commands.Update.UpdateRoleName;

public sealed class UpdateRoleNameCommandHandler(
       IRolesRepository rolesRepository,
       IAppUsersService userRepository,
       IHttpContextAccessor httpContextAccessor,
       IUnitOfWork unitOfWork,
       ICacheService cacheService) : ICommandHandler<UpdateRoleNameCommand, UpdateRoleNameCommandResponse>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IAppUsersService _userService = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<UpdateRoleNameCommandResponse>> Handle(UpdateRoleNameCommand request, CancellationToken cancellationToken)
    {
        var role = await _rolesRepository.GetAsync(
             predicate: r => r.Id == request.RoleId,
             include: r => r.Permissions,
             cancellationToken: cancellationToken);

        if (role is null)
        {
            return Result.NotFound();
        }

        role.ChangeName(request.Name);

        var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.GetAsync(
            predicate: u => u.IdentityId == identityId,
            includeSoftDeleted: false,
            cancellationToken: cancellationToken,
            include: [
                u => u.IdentityUser]);

        role.UpdatedBy = user.IdentityUser.Email;

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
        IPaginatedList<AppUser> usersBatch;

        do
        {
            usersBatch = await _userService.GetAllAsync(
                index: pageIndex,
                size: batchSize,
                includeSoftDeleted: false,
                predicate: u => u.Roles.Any(r => r.Id == roleId),
                cancellationToken);

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
