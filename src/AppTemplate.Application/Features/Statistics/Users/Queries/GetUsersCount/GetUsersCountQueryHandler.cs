using AppTemplate.Application.Repositories;
using Ardalis.Result;
using MediatR;

namespace AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;

public sealed class GetUsersCountQueryHandler(IAppUsersRepository userRepository) : IRequestHandler<GetUsersCountQuery, Result<GetUsersCountQueryResponse>>
{
  private readonly IAppUsersRepository _userRepository = userRepository;

  public async Task<Result<GetUsersCountQueryResponse>> Handle(GetUsersCountQuery request, CancellationToken cancellationToken)
  {
    int count = await _userRepository.GetUsersCountAsync(cancellationToken: cancellationToken);

    var response = new GetUsersCountQueryResponse(count);

    return Result.Success(response);
  }
}
