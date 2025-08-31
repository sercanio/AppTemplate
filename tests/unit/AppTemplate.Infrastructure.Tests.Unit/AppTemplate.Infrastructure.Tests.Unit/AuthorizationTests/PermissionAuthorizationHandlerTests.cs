using AppTemplate.Core.Infrastructure.Authorization;
using AppTemplate.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AppTemplate.Infrastructure.Tests.Unit.AuthorizationTests;

public class PermissionAuthorizationHandlerTests
{
  [Fact]
  public async Task HandleRequirementAsync_Succeeds_WhenUserHasPermissionClaim()
  {
    // Arrange
    var requirement = new PermissionRequirement("feature.read");
    var identity = new ClaimsIdentity(new[] { new Claim("permission", "feature.read") });
    var principal = new ClaimsPrincipal(identity);
    var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

    var handler = new PermissionAuthorizationHandler();

    // Act
    await handler.HandleAsync(context);

    // Assert
    Assert.True(context.HasSucceeded);
  }

  [Fact]
  public async Task HandleRequirementAsync_DoesNotSucceed_WhenUserDoesNotHavePermissionClaim()
  {
    // Arrange
    var requirement = new PermissionRequirement("feature.write");
    var identity = new ClaimsIdentity(new[] { new Claim("permission", "feature.read") });
    var principal = new ClaimsPrincipal(identity);
    var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

    var handler = new PermissionAuthorizationHandler();

    // Act
    await handler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
  }

  [Fact]
  public async Task HandleRequirementAsync_DoesNotSucceed_WhenUserHasNoPermissionClaims()
  {
    // Arrange
    var requirement = new PermissionRequirement("feature.read");
    var identity = new ClaimsIdentity();
    var principal = new ClaimsPrincipal(identity);
    var context = new AuthorizationHandlerContext(new[] { requirement }, principal, null);

    var handler = new PermissionAuthorizationHandler();

    // Act
    await handler.HandleAsync(context);

    // Assert
    Assert.False(context.HasSucceeded);
  }
}
