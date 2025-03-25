using Microsoft.AspNetCore.Authorization;
using Myrtus.Clarity.Core.Infrastructure.Authorization;

namespace AppTemplate.Infrastructure.Autorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == requirement.Permission && c.Value.Equals("true", StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
