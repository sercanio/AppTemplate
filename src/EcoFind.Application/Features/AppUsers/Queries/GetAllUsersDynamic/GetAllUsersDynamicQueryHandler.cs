using Ardalis.Result;
using EcoFind.Application.Features.AppUsers.Queries.GetLoggedInUser;
using EcoFind.Application.Repositories;
using EcoFind.Domain.AppUsers;
using MediatR;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Collections.ObjectModel;

namespace EcoFind.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed class GetAllUsersDynamicQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersDynamicQuery, Result<IPaginatedList<GetAllUsersDynamicQueryResponse>>>
{
    private readonly IAppUsersRepository _userRepository = userRepository;

    public async Task<Result<IPaginatedList<GetAllUsersDynamicQueryResponse>>> Handle(GetAllUsersDynamicQuery request, CancellationToken cancellationToken)
    {

        IPaginatedList<AppUser> users = await _userRepository.GetAllDynamicAsync(
            request.DynamicQuery,
            request.PageIndex,
            request.PageSize,
            includeSoftDeleted: false,
            cancellationToken,
            include: [
                user => user.IdentityUser,
                    user => user.Roles]);

        List<GetAllUsersDynamicQueryResponse> mappedUsers = users.Items.Select(
            user => new GetAllUsersDynamicQueryResponse(
            user.Id,
            email: user.IdentityUser?.Email ?? string.Empty,
            userName: user.IdentityUser?.UserName ?? string.Empty,
            roles: new Collection<LoggedInUserRolesDto>(
                        user.Roles?.Where(
                            role => role.DeletedOnUtc == null).Select(
                            role => new LoggedInUserRolesDto(role.Id, role.Name.Value)).ToList()
                            ?? new List<LoggedInUserRolesDto>())
                    )).ToList();

        PaginatedList<GetAllUsersDynamicQueryResponse> paginatedList = new(
            mappedUsers,
            users.TotalCount,
            request.PageIndex,
            request.PageSize
        );

        return Result.Success<IPaginatedList<GetAllUsersDynamicQueryResponse>>(paginatedList);
    }
}
