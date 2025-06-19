using AppTemplate.Application.Repositories;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;

public sealed class GetUsersCountQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetUsersCountQuery, Result<GetUsersCountQueryResponse>>
{
    private readonly IAppUsersRepository _userRepository = userRepository;

    public async Task<Result<GetUsersCountQueryResponse>> Handle(GetUsersCountQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(
            pageIndex: 0,
            pageSize: 1,
            includeSoftDeleted: false,
            cancellationToken: cancellationToken);

        var response = new GetUsersCountQueryResponse(users.TotalCount);

        return Result.Success(response);
    }
}
