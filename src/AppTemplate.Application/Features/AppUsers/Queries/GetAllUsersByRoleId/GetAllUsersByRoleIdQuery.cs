using AppTemplate.Application.Data.Pagination;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

public sealed record GetAllUsersByRoleIdQuery(
    int PageIndex,
    int PageSize,
    Guid RoleId) : IRequest<Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>>;