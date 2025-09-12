using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;

public sealed record GetAllPermissionsQuery(
    int PageIndex,
    int PageSize) : IQuery<IPaginatedList<GetAllPermissionsQueryResponse>>;
