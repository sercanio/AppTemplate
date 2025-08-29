using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed class GetAllUsersDynamicQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersDynamicQuery, Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>
{
  private readonly IAppUsersRepository _userRepository = userRepository;

  public async Task<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>> Handle(GetAllUsersDynamicQuery request, CancellationToken cancellationToken)
  {

    PaginatedList<AppUser> users = await _userRepository.GetAllDynamicAsync(
        dynamicQuery: request.DynamicQuery,
        pageIndex: request.PageIndex,
        pageSize: request.PageSize,
        includeSoftDeleted: false,
        include: query => query.
            Include(u => u.IdentityUser).
            Include(u => u.Roles.Where(r => r.DeletedOnUtc == null)),
        cancellationToken: cancellationToken);

    List<GetAllUsersDynamicQueryResponse> mappedUsers = users.Items.Select(
        user => new GetAllUsersDynamicQueryResponse(
        user.Id,
        userName: user.IdentityUser?.UserName ?? string.Empty,
        roles: new Collection<LoggedInUserRolesDto>(
                    user.Roles?.Where(
                        role => role.DeletedOnUtc == null).Select(
                        role => new LoggedInUserRolesDto(role.Id, role.Name.Value, role.DisplayName.Value)).ToList()
                        ?? new List<LoggedInUserRolesDto>())
                )).ToList();

    PaginatedList<GetAllUsersDynamicQueryResponse> paginatedList = new(
        mappedUsers,
        users.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success<PaginatedList<GetAllUsersDynamicQueryResponse>>(paginatedList);
  }
}
