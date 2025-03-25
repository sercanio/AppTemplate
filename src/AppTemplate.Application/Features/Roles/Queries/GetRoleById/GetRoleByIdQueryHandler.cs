using Ardalis.Result;
using AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using System.Collections.ObjectModel;
using System.Data;

namespace AppTemplate.Application.Features.Roles.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler(IRolesRepository roleRepository) : IQueryHandler<GetRoleByIdQuery, GetRoleByIdQueryResponse>
{
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task<Result<GetRoleByIdQueryResponse>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        Role? role = await _roleRepository.GetAsync(
            predicate: role => role.Id == request.RoleId,
            include: role => role.Permissions);

        if (role is null)
        {
            return Result.NotFound(RoleErrors.NotFound.Name);
        }

        List<GetRoleByIdPermissionResponseDto> mappedPermissions = role.Permissions.Select(permission =>
            new GetRoleByIdPermissionResponseDto(permission.Name)).ToList();

        GetRoleByIdQueryResponse response = new(
            role.Id,
            role.Name.Value,
            role.IsDefault.Value,
            new Collection<GetRoleByIdPermissionResponseDto>(mappedPermissions));

        return Result.Success(response);
    }
}
