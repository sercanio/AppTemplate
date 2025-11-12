using System.Collections.ObjectModel;
using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetUser;

public sealed class GetUserQueryHandler(IAppUsersRepository userRepository) : IQueryHandler<GetUserQuery, GetUserQueryResponse>
{
  private readonly IAppUsersRepository _userRepository = userRepository;

  public async Task<Result<GetUserQueryResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
  {
    var result = await _userRepository.GetUserByIdWithIdentityAndRrolesAsync(request.UserId, cancellationToken);

    if (!result.IsSuccess || result.Value is null)
    {
      return Result<GetUserQueryResponse>.NotFound(AppUserErrors.NotFound.Name);
    }

    var user = result.Value;

    List<GetRoleByIdQueryResponse> mappedRoles = user.Roles.Select(role =>
        new GetRoleByIdQueryResponse(role.Id, role.Name.Value, role.DisplayName.Value, role.IsDefault)).ToList();

    GetUserQueryResponse response = new(
        user.Id,
        user.IdentityUser.UserName!,
        new Collection<GetRoleByIdQueryResponse>(mappedRoles));

    return Result.Success(response);
  }
}
