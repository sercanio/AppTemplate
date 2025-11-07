using System.Collections.ObjectModel;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed record GetAllUsersDynamicQueryResponse
{
  public Guid Id { get; init; }
  public string UserName { get; init; }
  public bool EmailConfirmed { get; init; }
  public DateTime JoinDate { get; init; }
  public ICollection<LoggedInUserRolesDto> Roles { get; init; } = new Collection<LoggedInUserRolesDto>();

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
}
