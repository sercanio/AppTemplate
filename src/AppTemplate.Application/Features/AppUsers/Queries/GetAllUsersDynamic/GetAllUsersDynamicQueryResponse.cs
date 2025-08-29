using System.Collections.ObjectModel;
using AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetAllUsersDynamic;

public sealed record GetAllUsersDynamicQueryResponse
{
  public Guid Id { get; init; }
  public string UserName { get; init; }
  public ICollection<LoggedInUserRolesDto> Roles { get; init; } = [];

  public GetAllUsersDynamicQueryResponse(
      Guid id,
      string userName,
      Collection<LoggedInUserRolesDto> roles)
  {
    Id = id;
    UserName = userName;
    Roles = roles;
  }
}