using AppTemplate.Application.Repositories;
using AppTemplate.Core.Application.Abstractions.Messaging;
using AppTemplate.Core.Application.Abstractions.Pagination;
using AppTemplate.Core.Infrastructure.Pagination;
using Ardalis.Result;

namespace AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;

public class GetallPermissionsQueryHandler(IPermissionsRepository permissionRepository)
    : IQueryHandler<GetAllPermissionsQuery, IPaginatedList<GetAllPermissionsQueryResponse>>
{
    private readonly IPermissionsRepository _permissionRepository = permissionRepository;

    public async Task<Result<IPaginatedList<GetAllPermissionsQueryResponse>>> Handle(
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

        return Result.Success<IPaginatedList<GetAllPermissionsQueryResponse>>(paginatedList);
    }
}
