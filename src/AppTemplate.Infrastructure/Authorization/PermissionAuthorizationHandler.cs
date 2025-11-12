using AppTemplate.Application.Services.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AppTemplate.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      PermissionRequirement requirement)
  {
    if (context.User.HasClaim("permission", requirement.Permission))
      context.Succeed(requirement);

    return Task.CompletedTask;
  }
}
