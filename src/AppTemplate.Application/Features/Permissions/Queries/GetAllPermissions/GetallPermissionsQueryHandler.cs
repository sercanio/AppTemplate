using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Messages;
using Ardalis.Result;

namespace AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;

public class GetallPermissionsQueryHandler(IPermissionsRepository permissionRepository)
    : IQueryHandler<GetAllPermissionsQuery, PaginatedList<GetAllPermissionsQueryResponse>>
{
  private readonly IPermissionsRepository _permissionRepository = permissionRepository;

  public async Task<Result<PaginatedList<GetAllPermissionsQueryResponse>>> Handle(
      GetAllPermissionsQuery request,
      CancellationToken cancellationToken)
  {
    var result = await _permissionRepository.GetAllPermissionsAsync(
        pageIndex: request.PageIndex,
        pageSize: request.PageSize,
        cancellationToken: cancellationToken);

    if (!result.IsSuccess || result.Value is null)
    {
      return Result.Error("Could not retrieve permissions.");
    }

    var permissions = result.Value;

    List<GetAllPermissionsQueryResponse> mappedPermissions = permissions.Items.Select(permission =>
        new GetAllPermissionsQueryResponse(permission.Id, permission.Feature, permission.Name)).ToList();

    PaginatedList<GetAllPermissionsQueryResponse> paginatedList = new(
        mappedPermissions,
        permissions.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success<PaginatedList<GetAllPermissionsQueryResponse>>(paginatedList);
  }
}
