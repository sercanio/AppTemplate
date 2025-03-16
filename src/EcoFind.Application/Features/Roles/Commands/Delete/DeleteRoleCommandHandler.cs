using Ardalis.Result;
using EcoFind.Application.Repositories;
using EcoFind.Application.Services.AppUsers;
using EcoFind.Domain.Roles;
using Microsoft.AspNetCore.Http;
using Myrtus.Clarity.Core.Application.Abstractions.Caching;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Domain.Abstractions;
using System.Security.Claims;

namespace EcoFind.Application.Features.Roles.Commands.Delete;

public sealed class DeleteRoleCommandHandler(
    IRolesRepository rolesRepository,
    IAppUsersService userRepository,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    ICacheService cacheService) : ICommandHandler<DeleteRoleCommand, DeleteRoleCommandResponse>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IAppUsersService _userService = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Result<DeleteRoleCommandResponse>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _rolesRepository.GetAsync(
            predicate: role => role.Id == request.RoleId,
            cancellationToken: cancellationToken);

        if (role is null)
        {
            return Result.NotFound(RoleErrors.NotFound.Name);
        }

        var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.GetAsync(
            predicate: u => u.IdentityId == identityId,
            includeSoftDeleted: false,
            cancellationToken: cancellationToken,
            include: [
                u => u.IdentityUser]);

        if (user is not null)
        {
            role.UpdatedBy = user.IdentityUser.Email;
        }

        try
        {
            _ = Role.Delete(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cacheService.RemoveAsync($"roles-{role.Id}", cancellationToken);

            DeleteRoleCommandResponse response = new(role.Id, role.Name.Value);
            return Result.Success(response);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Forbidden(ex.Message);
        }
    }
}
