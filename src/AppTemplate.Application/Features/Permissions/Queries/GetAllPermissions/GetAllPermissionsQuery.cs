using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace AppTemplate.Application.Features.Permissions.Queries.GetAllPermissions;

public sealed record GetAllPermissionsQuery(
    int PageIndex,
    int PageSize) : IQuery<IPaginatedList<GetAllPermissionsQueryResponse>>;
