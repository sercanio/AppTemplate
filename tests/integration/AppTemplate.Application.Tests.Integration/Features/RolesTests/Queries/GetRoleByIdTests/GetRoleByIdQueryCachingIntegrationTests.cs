using AppTemplate.Application.Features.Roles.Queries.GetRoleById;
using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Tests.Integration.Features.RolesTests.Queries.GetRoleByIdTests;

[Trait("Category", "Integration")]
public class GetRoleByIdQueryCachingIntegrationTests
{
  [Fact]
  public void GetRoleByIdQuery_CacheKey_IsUnique()
  {
    // Arrange
    var roleId1 = Guid.NewGuid();
    var roleId2 = Guid.NewGuid();
    var query1 = new GetRoleByIdQuery(roleId1);
    var query2 = new GetRoleByIdQuery(roleId2);

    // Act
    var cacheKey1 = query1.CacheKey;
    var cacheKey2 = query2.CacheKey;

    // Assert
    Assert.NotEqual(cacheKey1, cacheKey2);
  }

  [Fact]
  public void GetRoleByIdQuery_CacheKey_SameForSameRole()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query1 = new GetRoleByIdQuery(roleId);
    var query2 = new GetRoleByIdQuery(roleId);

    // Act
    var cacheKey1 = query1.CacheKey;
    var cacheKey2 = query2.CacheKey;

    // Assert
    Assert.Equal(cacheKey1, cacheKey2);
  }

  [Fact]
  public void GetRoleByIdQuery_Expiration_IsNull_ForNoCacheExpiration()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetRoleByIdQuery(roleId);

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.Null(expiration);
  }

  [Fact]
  public void GetRoleByIdQuery_ImplementsICachedQueryInterface()
  {
    // Arrange
    var roleId = Guid.NewGuid();
    var query = new GetRoleByIdQuery(roleId);

    // Act
    var isCachedQuery = query is ICachedQuery<GetRoleByIdQueryResponse>;

    // Assert
    Assert.True(isCachedQuery);
    Assert.NotNull(query.CacheKey);
  }

  [Fact]
  public void GetRoleByIdQuery_CacheKey_FollowsExpectedFormat()
  {
    // Arrange
    var roleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    var query = new GetRoleByIdQuery(roleId);

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal("roles-a1b2c3d4-e5f6-7890-abcd-ef1234567890", cacheKey);
    Assert.Matches(@"^roles-[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", cacheKey);
  }

  [Fact]
  public void GetRoleByIdQuery_MultipleQueries_GenerateConsistentCacheKeys()
  {
    // Arrange
    var roleIds = new[]
    {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

    // Act
    var cacheKeys = roleIds.Select(id => new GetRoleByIdQuery(id).CacheKey).ToList();

    // Assert
    Assert.Equal(3, cacheKeys.Count);
    Assert.Equal(3, cacheKeys.Distinct().Count()); // All unique
    Assert.All(cacheKeys, key => Assert.StartsWith("roles-", key));
  }
}