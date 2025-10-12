using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.Roles.Queries.GetAllRoles;

public sealed record GetAllRolesQuery(int PageIndex, int PageSize) : IQuery<PaginatedList<GetAllRolesQueryResponse>>;
