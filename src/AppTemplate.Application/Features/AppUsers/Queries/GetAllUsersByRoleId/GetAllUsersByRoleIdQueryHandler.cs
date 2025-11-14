using System.Collections.ObjectModel;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

public sealed class GetAllUsersByRoleIdQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersByRoleIdQuery, Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>>
{
  private readonly IAppUsersRepository _userRepository = userRepository;

  public async Task<Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>> Handle(GetAllUsersByRoleIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _userRepository.GetAllUsersByRoleIdWithIdentityAndRolesAsync(
        request.RoleId,
        request.PageIndex,
        request.PageSize,
        cancellationToken);

    if (!result.IsSuccess || result.Value is null)
    {
      return Result<PaginatedList<GetAllUsersByRoleIdQueryResponse>>.Error("Could not retrieve users.");
    }

    var users = result.Value;

    List<GetAllUsersByRoleIdQueryResponse> mappedUsers = users.Items.Select(
        user => new GetAllUsersByRoleIdQueryResponse(
            user.Id,
            userName: user.IdentityUser?.UserName ?? string.Empty,
            emailConfirmed: user.IdentityUser?.EmailConfirmed ?? false,
            joinDate: user.CreatedOnUtc,
            roles: new Collection<LoggedInUserRolesDto>(
                user.Roles?
                    .Where(role => role.DeletedOnUtc == null)
                    .Select(role => new LoggedInUserRolesDto(role.Id, role.Name.Value, role.DisplayName.Value))
                    .ToList() ?? new List<LoggedInUserRolesDto>()))
        ).ToList();

    PaginatedList<GetAllUsersByRoleIdQueryResponse> paginatedList = new(
        mappedUsers,
        users.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success(paginatedList);
  }
}
