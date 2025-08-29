using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record GetAllUsersQueryResponse
{
  public Guid Id { get; set; }
  public string? UserName { get; set; }
  public bool EmailConfirmed { get; set; }
  public ICollection<LoggedInUserRolesDto> Roles { get; set; } = new List<LoggedInUserRolesDto>();

  public GetAllUsersQueryResponse(
      Guid id,
      string? userName,
      bool emailConfirmed,
      Collection<LoggedInUserRolesDto>? roles = null)
  {
    Id = id;
    UserName = userName;
    EmailConfirmed = emailConfirmed;
    Roles = roles ?? new Collection<LoggedInUserRolesDto>();
  }

  public GetAllUsersQueryResponse() { }
}
