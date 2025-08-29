using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

public sealed record GetAllUsersByRoleIdQuery(
    int PageIndex,
    int PageSize,
    Guid RoleId) : IRequest<Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>>;