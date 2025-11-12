using System.Security.Claims;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Messages;
using AppTemplate.Domain.AppUsers;
using Ardalis.Result;
using Microsoft.AspNetCore.Http;

public sealed class GetLoggedInUserQueryHandler : IQueryHandler<GetLoggedInUserQuery, UserResponse>
{
  private readonly IAppUsersRepository _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public GetLoggedInUserQueryHandler(
      IAppUsersRepository userRepository,
      IHttpContextAccessor httpContextAccessor)
  {
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<UserResponse>> Handle(GetLoggedInUserQuery request, CancellationToken cancellationToken)
  {
    string? userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
      return Result<UserResponse>.NotFound("User not found in claims");
    }

    var result = await _userRepository.GetUserByIdentityIdWithIdentityAndRolesAsync(userId, cancellationToken);

    if (!result.IsSuccess || result.Value is null)
    {
      return Result<UserResponse>.NotFound(AppUserErrors.NotFound.Name);
    }

    var user = result.Value;

    var mappedRoles = user.Roles
        .Where(role => role.DeletedOnUtc == null)
        .Select(role => new LoggedInUserRolesDto(role.Id, role.Name.Value, role.DisplayName.Value))
        .ToList();

    UserResponse userResponse = new()
    {
      Email = user.IdentityUser.Email!,
      UserName = user.IdentityUser.UserName!,
      Roles = mappedRoles,
      NotificationPreference = user.NotificationPreference,
      EmailConfirmed = user.IdentityUser.EmailConfirmed
    };

    return Result.Success(userResponse);
  }
}