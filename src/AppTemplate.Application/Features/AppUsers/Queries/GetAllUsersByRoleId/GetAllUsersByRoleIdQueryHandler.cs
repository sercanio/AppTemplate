using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AppTemplate.Core.Infrastructure.Pagination;
using System.Collections.ObjectModel;
  
namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

public sealed class GetAllUsersByRoleIdQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersByRoleIdQuery, Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>>
{
  private readonly IAppUsersRepository _userRepository = userRepository;

  public async Task<Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>> Handle(GetAllUsersByRoleIdQuery request, CancellationToken cancellationToken)
  {
    PaginatedList<AppUser> users = await _userRepository.GetAllAsync(
        pageIndex: request.PageIndex,
        pageSize: request.PageSize,
        includeSoftDeleted: false,
        predicate: user => user.Roles.Any(role => role.Id == request.RoleId),
        include: query => query.
            Include(u => u.IdentityUser).
            Include(u => u.Roles),
        cancellationToken: cancellationToken);

    List<GetAllUsersByRoleIdQueryResponse> mappedUsers = users.Items.Select(
        user => new GetAllUsersByRoleIdQueryResponse(
            user.Id,
            user.IdentityUser.UserName!,
            new Collection<LoggedInUserRolesDto>(
                user.Roles.Where(
                    role => role.DeletedOnUtc == null).Select(
                    role => new LoggedInUserRolesDto(role.Id, role.Name.Value, role.DisplayName.Value)).ToList())
    )).ToList();

    PaginatedList<GetAllUsersByRoleIdQueryResponse> paginatedList = new(
        mappedUsers,
        users.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success<PaginatedList<GetAllUsersByRoleIdQueryResponse>>(paginatedList);
  }
}
