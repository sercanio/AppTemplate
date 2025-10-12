using Microsoft.AspNetCore.Authorization;

namespace AppTemplate.Application.Services.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
  public HasPermissionAttribute(string permission)
      : base(permission)
  {
  }
}