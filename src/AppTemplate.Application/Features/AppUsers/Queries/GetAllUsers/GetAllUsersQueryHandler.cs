using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Features.Users.Queries.GetAllUsers;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Collections.ObjectModel;
using System.Data;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<GetAllUsersQueryResponse>>>
{
  public async Task<Result<PaginatedList<GetAllUsersQueryResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
  {
    PaginatedList<AppUser> users = await userRepository.GetAllAsync(
        pageIndex: request.PageIndex,
        pageSize: request.PageSize,
        include: query => query.
            Include(u => u.IdentityUser).
            Include(u => u.Roles.Where(r => r.DeletedOnUtc == null)),
        includeSoftDeleted: false,
        cancellationToken: cancellationToken);

    List<GetAllUsersQueryResponse> mappedUsers = users.Items.Select(
        user => new GetAllUsersQueryResponse(
        user.Id,
        userName: user.IdentityUser!.UserName ?? string.Empty,
        emailConfirmed: user.IdentityUser!.EmailConfirmed,
        roles: new Collection<LoggedInUserRolesDto>(
                    user.Roles?.Where(
                        role => role.DeletedOnUtc == null).Select(
                        role => new LoggedInUserRolesDto(role.Id, role.Name.Value, role.DisplayName.Value)).ToList() ?? new List<LoggedInUserRolesDto>())
    )).ToList();

    PaginatedList<GetAllUsersQueryResponse> paginatedList = new(
        mappedUsers,
        users.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success<PaginatedList<GetAllUsersQueryResponse>>(paginatedList);
  }
}
