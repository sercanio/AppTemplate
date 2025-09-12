using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AppTemplate.Application.Features.Roles.Commands.Create;

public sealed class CreateRoleCommandHander(
    IRolesRepository rolesRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IAppUsersRepository usersRepository,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<CreateRoleCommand, CreateRoleCommandResponse>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IAppUsersRepository _usersRepository = usersRepository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<CreateRoleCommandResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        bool nameExists = await _rolesRepository.ExistsAsync(
            predicate: role => role.Name == new RoleName(request.Name),
            cancellationToken: cancellationToken);

        if (nameExists)
        {
            return Result.Conflict(RoleErrors.Overlap.Name);
        }

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

        var role = Role.Create(request.Name, request.DisplayName, userResult.Value.Id);

        await _rolesRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"roles-{role.Id}", cancellationToken);

        var response = new CreateRoleCommandResponse(role.Id, role.Name.Value);
        return Result.Success(response);
    }
}