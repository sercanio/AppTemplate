using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using Ardalis.Result;
using MediatR;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed class GetAllUsersDynamicQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetAllUsersDynamicQuery, Result<PaginatedList<GetAllUsersDynamicQueryResponse>>>
{
    private readonly IAppUsersRepository _userRepository = userRepository;

    public async Task<Result<PaginatedList<GetAllUsersDynamicQueryResponse>>> Handle(GetAllUsersDynamicQuery request, CancellationToken cancellationToken)
    {
        var result = await _userRepository.GetAllUsersDynamicWithIdentityAndRolesAsync(
            request.DynamicQuery,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return Result<PaginatedList<GetAllUsersDynamicQueryResponse>>.Error("Could not retrieve users.");
        }

        var users = result.Value;

        List<GetAllUsersDynamicQueryResponse> mappedUsers = users.Items.Select(
            user => new GetAllUsersDynamicQueryResponse(
                user.Id,
                userName: user.IdentityUser?.UserName ?? string.Empty,
                roles: new Collection<LoggedInUserRolesDto>(
                    user.Roles?
                        .Where(role => role.DeletedOnUtc == null)
                        .Select(role => new LoggedInUserRolesDto(role.Id, role.Name.Value, role.DisplayName.Value))
                        .ToList() ?? new List<LoggedInUserRolesDto>())
            )).ToList();

        PaginatedList<GetAllUsersDynamicQueryResponse> paginatedList = new(
            mappedUsers,
            users.TotalCount,
            request.PageIndex,
            request.PageSize
        );

        return Result.Success(paginatedList);
    }
}
