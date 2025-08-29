using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Core.Application.Abstractions.Messaging;
using AppTemplate.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int PageIndex,
    int PageSize) : IQuery<PaginatedList<GetAllUsersQueryResponse>>;