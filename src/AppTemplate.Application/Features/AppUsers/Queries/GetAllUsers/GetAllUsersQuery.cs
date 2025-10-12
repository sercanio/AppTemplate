using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Services.Messages;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int PageIndex,
    int PageSize) : IQuery<PaginatedList<GetAllUsersQueryResponse>>;