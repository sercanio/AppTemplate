using Microsoft.AspNetCore.Authorization;

namespace AppTemplate.Application.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
  public HasPermissionAttribute(string permission)
      : base(permission)
  {
  }
}