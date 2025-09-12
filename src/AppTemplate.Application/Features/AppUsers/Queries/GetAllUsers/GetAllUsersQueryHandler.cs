using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using Ardalis.Result;
using MediatR;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<GetAllUsersQueryResponse>>>
{
  public async Task<Result<PaginatedList<GetAllUsersQueryResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
  {
    var result = await userRepository.GetAllUsersWithIdentityAndRolesAsync(
        request.PageIndex,
        request.PageSize,
        cancellationToken);

    if (!result.IsSuccess || result.Value is null)
    {
      return Result<PaginatedList<GetAllUsersQueryResponse>>.Error("Could not retrieve users.");
    }

    var users = result.Value;

    List<GetAllUsersQueryResponse> mappedUsers = users.Items.Select(
        user => new GetAllUsersQueryResponse(
            user.Id,
            userName: user.IdentityUser?.UserName ?? string.Empty,
            emailConfirmed: user.IdentityUser?.EmailConfirmed ?? false,
            roles: new Collection<LoggedInUserRolesDto>(
                user.Roles?
                .Where(
                    role => role.DeletedOnUtc == null)
                .Select(
                    role => new LoggedInUserRolesDto(
                        role.Id,
                        role.Name.Value,
                        role.DisplayName.Value)).ToList()
                ?? new List<LoggedInUserRolesDto>())
        )).ToList();

    PaginatedList<GetAllUsersQueryResponse> paginatedList = new(
        mappedUsers,
        users.TotalCount,
        request.PageIndex,
        request.PageSize
    );

    return Result.Success(paginatedList);
  }
}
