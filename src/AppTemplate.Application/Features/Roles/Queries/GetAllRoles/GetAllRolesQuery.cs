using AppTemplate.Core.Application.Abstractions.Messaging;
using AppTemplate.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.Roles.Queries.GetAllRoles;

public sealed record GetAllRolesQuery(int PageIndex, int PageSize) : IQuery<PaginatedList<GetAllRolesQueryResponse>>;
