namespace AppTemplate.Domain.Roles;

public static class RoleErrors
{
    public static readonly DomainError NotFound = new(
        "Role.NotFound",
        404,
        "The role with the specified identifier was not found");

    public static readonly DomainError Overlap = new(
        "Role.Overlap",
        409,
        "The current role is overlapping with an existing one");
}
