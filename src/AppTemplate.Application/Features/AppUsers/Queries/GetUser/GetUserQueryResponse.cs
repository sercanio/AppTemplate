using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using System.Collections.ObjectModel;

namespace AppTemplate.Application.Features.AppUsers.Queries.GetUser;

public sealed record GetUserQueryResponse
{
  public Guid Id { get; set; }
  public string UserName { get; set; }
  public ICollection<GetRoleByIdQueryResponse> Roles { get; set; } = [];

  public GetUserQueryResponse(Guid id,
      string userName,
      Collection<GetRoleByIdQueryResponse> roles)
  {
    Id = id;
    UserName = userName;
    Roles = roles;
  }

  public GetUserQueryResponse(Guid id, string userName)
  {
    Id = id;
    UserName = userName;
  }

  public GetUserQueryResponse() { }
}
