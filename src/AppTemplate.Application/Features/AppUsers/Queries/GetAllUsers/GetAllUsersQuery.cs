using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int PageIndex,
    int PageSize) : IQuery<PaginatedList<GetAllUsersQueryResponse>>;