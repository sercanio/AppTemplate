using System.Security.Claims;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Infrastructure.Authorization;
using Moq;

namespace AppTemplate.Infrastructure.Tests.Unit.AuthorizationTests;

[Trait("Category", "Unit")]
public class CustomClaimsTransformationTests
{
  [Fact]
  public async Task TransformAsync_ReturnsPrincipalUnchanged_IfAlreadyHasRoleOrPermissionClaims()
  {
    var authServiceMock = new Mock<IAuthorizationService>();
    var transformation = new CustomClaimsTransformation(authServiceMock.Object);

    var identity = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "user"),
        new Claim(ClaimTypes.Role, "admin"),
        new Claim("permission", "perm1")
    }, "TestAuth");
    var principal = new ClaimsPrincipal(identity);

    var result = await transformation.TransformAsync(principal);

    Assert.Same(principal, result);
  }

  [Fact]
  public async Task TransformAsync_AddsRoleAndPermissionClaims_IfNotPresent()
  {
    // Arrange
    var rolesResponse = new UserRolesResponse
    {
      UserId = Guid.NewGuid(),
      Roles = [new Domain.Roles.Role(
            Guid.NewGuid(),
            new Domain.Roles.ValueObjects.RoleName("Admin"),
            new Domain.Roles.ValueObjects.RoleName("Admin"),
            null,
            false)]
    };
    var permissions = new HashSet<string> { "perm1", "perm2" };

    var authServiceMock = new Mock<IAuthorizationService>();
    authServiceMock.Setup(s => s.GetRolesForUserAsync(It.IsAny<string>()))
        .ReturnsAsync(rolesResponse);
    authServiceMock.Setup(s => s.GetPermissionsForUserAsync(It.IsAny<string>()))
        .ReturnsAsync(permissions);

    var transformation = new CustomClaimsTransformation(authServiceMock.Object);

    var identity = new ClaimsIdentity("TestAuth");
    identity.AddClaim(new Claim(ClaimTypes.Name, "user"));
    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "test-identity-id"));
    var principal = new ClaimsPrincipal(identity);

    // Act
    var result = await transformation.TransformAsync(principal);

    // Find the new identity (it will have the added claims)
    var newIdentity = result.Identities.FirstOrDefault(i =>
        i.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin") &&
        i.Claims.Any(c => c.Type == "permission" && c.Value == "perm1"));

    // Assert
    Assert.NotNull(newIdentity);
    Assert.Contains(newIdentity.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    Assert.Contains(newIdentity.Claims, c => c.Type == "permission" && c.Value == "perm1");
    Assert.Contains(newIdentity.Claims, c => c.Type == "permission" && c.Value == "perm2");
    Assert.Contains(newIdentity.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == rolesResponse.UserId.ToString());
  }

  [Fact]
  public async Task TransformAsync_AddsClaims_IfOnlyRoleOrPermissionPresent()
  {
    var rolesResponse = new UserRolesResponse
    {
      UserId = Guid.NewGuid(),
      Roles = [new Domain.Roles.Role(
            Guid.NewGuid(),
            new Domain.Roles.ValueObjects.RoleName("Admin"),
            new Domain.Roles.ValueObjects.RoleName("Admin"),
            null,
            false)]
    };
    var permissions = new HashSet<string> { "perm1", "perm2" };

    var authServiceMock = new Mock<IAuthorizationService>();
    authServiceMock.Setup(s => s.GetRolesForUserAsync(It.IsAny<string>()))
        .ReturnsAsync(rolesResponse);
    authServiceMock.Setup(s => s.GetPermissionsForUserAsync(It.IsAny<string>()))
        .ReturnsAsync(permissions);

    var transformation = new CustomClaimsTransformation(authServiceMock.Object);

    // Only role claim present
    var identityRole = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Role, "admin"),
        new Claim(ClaimTypes.NameIdentifier, "test-identity-id")
    }, "TestAuth");
    var principalRole = new ClaimsPrincipal(identityRole);

    var resultRole = await transformation.TransformAsync(principalRole);
    Assert.Same(principalRole, resultRole);

    // Only permission claim present
    var identityPerm = new ClaimsIdentity(new[]
    {
        new Claim("permission", "perm1"),
        new Claim(ClaimTypes.NameIdentifier, "test-identity-id")
    }, "TestAuth");
    var principalPerm = new ClaimsPrincipal(identityPerm);

    var resultPerm = await transformation.TransformAsync(principalPerm);
    Assert.Same(principalPerm, resultPerm);
  }
}