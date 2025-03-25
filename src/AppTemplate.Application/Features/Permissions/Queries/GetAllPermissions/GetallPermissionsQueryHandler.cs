using Ardalis.Result;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.Roles;

namespace AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;

public class GetallPermissionsQueryHandler(IPermissionsRepository permissionRepository)
            : IQueryHandler<GetAllPermissionsQuery, IPaginatedList<GetAllPermissionsQueryResponse>>
{
    private readonly IPermissionsRepository _permissionRepository = permissionRepository;

    public async Task<Result<IPaginatedList<GetAllPermissionsQueryResponse>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        IPaginatedList<Permission> permissions = await _permissionRepository.GetAllAsync(
            pageIndex: request.PageIndex,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

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
