using System.Collections.ObjectModel;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersByRoleId;

public sealed record GetAllUsersByRoleIdQueryResponse
{
  public Guid Id { get; set; }
  public string UserName { get; set; } = string.Empty;
  public bool EmailConfirmed { get; set; }
  public DateTime JoinDate { get; set; }
  public ICollection<LoggedInUserRolesDto> Roles { get; set; } = new List<LoggedInUserRolesDto>();

  public GetAllUsersByRoleIdQueryResponse(
      Guid id,
      string userName,
      Collection<LoggedInUserRolesDto> roles)
  {
    Id = id;
    UserName = userName;
    Roles = roles;
  }

  // Preserve existing overload used elsewhere
  public GetAllUsersByRoleIdQueryResponse(
      Guid id, string email, string userName)
  {
    Id = id;
    UserName = userName;
  }

  // New constructor that includes EmailConfirmed and JoinDate and preserves Roles defaulting
  public GetAllUsersByRoleIdQueryResponse(
      Guid id,
      string userName,
      bool emailConfirmed,
      DateTime joinDate,
      Collection<LoggedInUserRolesDto>? roles = null)
  {
    Id = id;
    UserName = userName;
    EmailConfirmed = emailConfirmed;
    JoinDate = joinDate;
    Roles = roles ?? new Collection<LoggedInUserRolesDto>();
  }

  public GetAllUsersByRoleIdQueryResponse() { }
}
