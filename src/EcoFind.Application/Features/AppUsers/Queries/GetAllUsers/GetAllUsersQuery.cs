using EcoFind.Application.Features.AppUsers.Queries.GetLoggedInUser;
using Myrtus.Clarity.Core.Application.Abstractions.Messaging;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;

namespace EcoFind.Application.Features.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int PageIndex,
    int PageSize) : IQuery<IPaginatedList<GetAllUsersQueryResponse>>;