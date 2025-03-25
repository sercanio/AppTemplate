﻿using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ardalis.Result;
using Myrtus.Clarity.Core.Application.Abstractions.Caching;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Domain.Abstractions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Domain.Roles;
using AppTemplate.Domain.Roles.ValueObjects;

namespace AppTemplate.Application.Features.Roles.Commands.Create;

public sealed class CreateRoleCommandHander(
    IRolesRepository rolesRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IAppUsersService usersService,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<CreateRoleCommand, CreateRoleCommandResponse>
{
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IAppUsersService _usersService = usersService;
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

        var role = Role.Create(request.Name);

        var identityId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _usersService.GetAsync(
            predicate: u => u.IdentityId == identityId,
            includeSoftDeleted: false,
            cancellationToken: cancellationToken,
            include: [
                u => u.IdentityUser]);
        if (user is not null)
        {
            role.CreatedBy = user.IdentityUser.Email!;
        }

        await _rolesRepository.AddAsync(role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"roles-{role.Id}", cancellationToken);

        CreateRoleCommandResponse response = new(
            role.Id,
            role.Name.Value);

        return Result.Success(response);
    }
}