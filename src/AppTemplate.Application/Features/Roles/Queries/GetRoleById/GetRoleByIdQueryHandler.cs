using System.Collections.ObjectModel;
using AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain.Roles;
using Ardalis.Result;

namespace AppTemplate.Application.Features.Roles.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler(IRolesRepository roleRepository) : IQueryHandler<GetRoleByIdQuery, GetRoleByIdQueryResponse>
{
  private readonly IRolesRepository _roleRepository = roleRepository;

  public async Task<Result<GetRoleByIdQueryResponse>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _roleRepository.GetRoleByIdWithPermissionsAsync(request.RoleId, cancellationToken);

    if (!result.IsSuccess || result.Value is null)
    {
      return Result.NotFound(RoleErrors.NotFound.Name);
    }

    var role = result.Value;

    List<GetRoleByIdPermissionResponseDto> mappedPermissions = role.Permissions
        .Select(permission => new GetRoleByIdPermissionResponseDto(permission.Name))
        .ToList();

    GetRoleByIdQueryResponse response = new(
        role.Id,
        role.Name.Value,
        role.DisplayName.Value,
        role.IsDefault,
        new Collection<GetRoleByIdPermissionResponseDto>(mappedPermissions));

    return Result.Success(response);
  }
}
