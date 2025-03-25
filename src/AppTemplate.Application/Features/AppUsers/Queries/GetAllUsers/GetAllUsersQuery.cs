using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int PageIndex,
    int PageSize) : IQuery<IPaginatedList<GetAllUsersQueryResponse>>;