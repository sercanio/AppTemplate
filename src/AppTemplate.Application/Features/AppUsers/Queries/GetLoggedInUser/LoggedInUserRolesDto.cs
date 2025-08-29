namespace AppTemplate.Application.Features.AppUsers.Queries.GetLoggedInUser;

public sealed record LoggedInUserRolesDto
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public string DisplayName { get; set; }

  public LoggedInUserRolesDto(Guid id, string name, string displayName)
  {
    Id = id;
    Name = name;
    DisplayName = displayName;
  }
};