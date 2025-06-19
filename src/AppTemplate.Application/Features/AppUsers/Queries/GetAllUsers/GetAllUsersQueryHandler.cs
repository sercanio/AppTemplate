using System.Collections.ObjectModel;
using System.Data;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Features.Users.Queries.GetAllUsers;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersQuery, Result<IPaginatedList<GetAllUsersQueryResponse>>>
{
    public async Task<Result<IPaginatedList<GetAllUsersQueryResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        IPaginatedList<AppUser> users = await userRepository.GetAllAsync(
            pageIndex: request.PageIndex,
            pageSize: request.PageSize,
            includeSoftDeleted: false,
            include: [
                user => user.IdentityUser,
                user => user.Roles],
            cancellationToken: cancellationToken);

        List<GetAllUsersQueryResponse> mappedUsers = users.Items.Select(
            user => new GetAllUsersQueryResponse(
            user.Id,
            userName: user.IdentityUser?.UserName ?? string.Empty,
            roles: new Collection<LoggedInUserRolesDto>(
                        user.Roles?.Where(
                            role => role.DeletedOnUtc == null).Select(
                            role => new LoggedInUserRolesDto(role.Id, role.Name.Value)).ToList() ?? new List<LoggedInUserRolesDto>())
        )).ToList();

        PaginatedList<GetAllUsersQueryResponse> paginatedList = new(
            mappedUsers,
            users.TotalCount,
            request.PageIndex,
            request.PageSize
        );

        return Result.Success<IPaginatedList<GetAllUsersQueryResponse>>(paginatedList);
    }
}
