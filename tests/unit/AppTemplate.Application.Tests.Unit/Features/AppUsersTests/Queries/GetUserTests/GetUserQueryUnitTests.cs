using AppTemplate.Application.Features.AppUsers.Queries.GetUser;

namespace AppTemplate.Application.Tests.Unit.Features.AppUsersTests.Queries.GetUserTests;

[Trait("Category", "Unit")]
public class GetUserQueryUnitTests
{
    [Fact]
    public void GetUserQuery_Constructor_SetsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query = new GetUserQuery(userId);

        // Assert
        Assert.Equal(userId, query.UserId);
    }

    [Fact]
    public void GetUserQuery_CacheKey_GeneratesCorrectFormat()
    {
        // Arrange
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var query = new GetUserQuery(userId);
        var expectedCacheKey = $"users-{userId}";

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        Assert.Equal(expectedCacheKey, cacheKey);
        Assert.Equal("users-12345678-1234-1234-1234-123456789012", cacheKey);
    }

    [Fact]
    public void GetUserQuery_CacheKey_IsDifferentForDifferentUserIds()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var query1 = new GetUserQuery(userId1);
        var query2 = new GetUserQuery(userId2);

        // Act
        var cacheKey1 = query1.CacheKey;
        var cacheKey2 = query2.CacheKey;

        // Assert
        Assert.NotEqual(cacheKey1, cacheKey2);
        Assert.StartsWith("users-", cacheKey1);
        Assert.StartsWith("users-", cacheKey2);
    }

    [Fact]
    public void GetUserQuery_Expiration_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        // Act
        var expiration = query.Expiration;

        // Assert
        Assert.Null(expiration);
    }

    [Fact]
    public void GetUserQuery_Expiration_IsConsistent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        // Act
        var expiration1 = query.Expiration;
        var expiration2 = query.Expiration;

        // Assert
        Assert.Equal(expiration1, expiration2);
        Assert.Null(expiration1);
        Assert.Null(expiration2);
    }

    [Fact]
    public void GetUserQuery_ImplementsICachedQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var query = new GetUserQuery(userId);

        // Assert
        Assert.IsAssignableFrom<AppTemplate.Application.Services.Caching.ICachedQuery<GetUserQueryResponse>>(query);
    }

    [Fact]
    public void GetUserQuery_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query1 = new GetUserQuery(userId);
        var query2 = new GetUserQuery(userId);
        var query3 = new GetUserQuery(Guid.NewGuid());

        // Act & Assert
        Assert.Equal(query1, query2);
        Assert.NotEqual(query1, query3);
        Assert.True(query1 == query2);
        Assert.False(query1 == query3);
    }

    [Fact]
    public void GetUserQuery_GetHashCode_IsConsistent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query1 = new GetUserQuery(userId);
        var query2 = new GetUserQuery(userId);

        // Act
        var hashCode1 = query1.GetHashCode();
        var hashCode2 = query2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetUserQuery_ToString_ContainsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        // Act
        var stringRepresentation = query.ToString();

        // Assert
        Assert.Contains(userId.ToString(), stringRepresentation);
        Assert.Contains("GetUserQuery", stringRepresentation);
    }
}
