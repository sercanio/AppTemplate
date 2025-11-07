using System.Collections.ObjectModel;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed record GetAllUsersDynamicQueryResponse
{
  public Guid Id { get; init; }
  public string UserName { get; init; } = string.Empty;
  public bool EmailConfirmed { get; init; }
  public DateTime JoinDate { get; init; }
  public ICollection<LoggedInUserRolesDto> Roles { get; init; } = new Collection<LoggedInUserRolesDto>();

  // Preserve existing callers that construct with (Guid, string, Collection<LoggedInUserRolesDto>)
  // EmailConfirmed defaults to false and JoinDate defaults to DateTime.MinValue
  public GetAllUsersDynamicQueryResponse(
      Guid id,
      string userName,
      Collection<LoggedInUserRolesDto> roles)
  {
    Id = id;
    UserName = userName;
    Roles = roles ?? new Collection<LoggedInUserRolesDto>();
    EmailConfirmed = false;
    JoinDate = DateTime.MinValue;
  }

  // Constructor matching the test usage: (Guid, string, bool, DateTime, Collection<LoggedInUserRolesDto>)
  // Also used by handlers that populate all fields
  public GetAllUsersDynamicQueryResponse(
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

  // Parameterless ctor for deserialization / tests that may require it
  public GetAllUsersDynamicQueryResponse() { }
}
