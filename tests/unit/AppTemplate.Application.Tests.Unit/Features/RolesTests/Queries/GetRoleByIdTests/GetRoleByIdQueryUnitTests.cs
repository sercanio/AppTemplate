using AppTemplate.Application.Features.Roles.Queries.GetRoleById;

namespace AppTemplate.Application.Tests.Unit.Features.RolesTests.Queries.GetRoleByIdTests;

[Trait("Category", "Unit")]
public class GetRoleByIdQueryUnitTests
{
    [Fact]
    public void GetRoleByIdQuery_Constructor_SetsRoleId()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        var query = new GetRoleByIdQuery(roleId);

        // Assert
        Assert.Equal(roleId, query.RoleId);
    }

    [Fact]
    public void GetRoleByIdQuery_CacheKey_GeneratesCorrectFormat()
    {
        // Arrange
        var roleId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var query = new GetRoleByIdQuery(roleId);
        var expectedCacheKey = $"roles-{roleId}";

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        Assert.Equal(expectedCacheKey, cacheKey);
        Assert.Equal("roles-12345678-1234-1234-1234-123456789012", cacheKey);
    }

    [Fact]
    public void GetRoleByIdQuery_CacheKey_IsDifferentForDifferentRoleIds()
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
        Assert.StartsWith("roles-", cacheKey1);
        Assert.StartsWith("roles-", cacheKey2);
    }

    [Fact]
    public void GetRoleByIdQuery_Expiration_ReturnsNull()
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
    public void GetRoleByIdQuery_Expiration_IsConsistent()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query = new GetRoleByIdQuery(roleId);

        // Act
        var expiration1 = query.Expiration;
        var expiration2 = query.Expiration;

        // Assert
        Assert.Equal(expiration1, expiration2);
        Assert.Null(expiration1);
        Assert.Null(expiration2);
    }

    [Fact]
    public void GetRoleByIdQuery_ImplementsICachedQuery()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        var query = new GetRoleByIdQuery(roleId);

        // Assert
        Assert.IsAssignableFrom<AppTemplate.Application.Services.Caching.ICachedQuery<GetRoleByIdQueryResponse>>(query);
    }

    [Fact]
    public void GetRoleByIdQuery_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query1 = new GetRoleByIdQuery(roleId);
        var query2 = new GetRoleByIdQuery(roleId);
        var query3 = new GetRoleByIdQuery(Guid.NewGuid());

        // Act & Assert
        Assert.Equal(query1, query2);
        Assert.NotEqual(query1, query3);
        Assert.True(query1 == query2);
        Assert.False(query1 == query3);
    }

    [Fact]
    public void GetRoleByIdQuery_GetHashCode_IsConsistent()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query1 = new GetRoleByIdQuery(roleId);
        var query2 = new GetRoleByIdQuery(roleId);

        // Act
        var hashCode1 = query1.GetHashCode();
        var hashCode2 = query2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetRoleByIdQuery_ToString_ContainsRoleId()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query = new GetRoleByIdQuery(roleId);

        // Act
        var stringRepresentation = query.ToString();

        // Assert
        Assert.Contains(roleId.ToString(), stringRepresentation);
        Assert.Contains("GetRoleByIdQuery", stringRepresentation);
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
}