using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Authentication;

[Trait("Category", "Unit")]
public class GetAuthenticationStatisticsQueryUnitTests
{
  [Fact]
  public void GetAuthenticationStatisticsQuery_Constructor_CreatesInstance()
  {
    // Act
    var query = new GetAuthenticationStatisticsQuery();

    // Assert
    Assert.NotNull(query);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheKey_IsConstant()
  {
    // Arrange
    var query1 = new GetAuthenticationStatisticsQuery();
    var query2 = new GetAuthenticationStatisticsQuery();

    // Act
    var cacheKey1 = query1.CacheKey;
    var cacheKey2 = query2.CacheKey;

    // Assert
    Assert.Equal("authentication-statistics", cacheKey1);
    Assert.Equal("authentication-statistics", cacheKey2);
    Assert.Equal(cacheKey1, cacheKey2);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheKey_IsSameForAllInstances()
  {
    // Arrange
    var queries = new[]
    {
            new GetAuthenticationStatisticsQuery(),
            new GetAuthenticationStatisticsQuery(),
            new GetAuthenticationStatisticsQuery()
        };

    // Act
    var cacheKeys = queries.Select(q => q.CacheKey).ToList();

    // Assert
    Assert.All(cacheKeys, key => Assert.Equal("authentication-statistics", key));
    Assert.Equal(1, cacheKeys.Distinct().Count()); // All the same
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_IsOneMinute()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.Equal(TimeSpan.FromMinutes(1), expiration);
    Assert.Equal(60, expiration.Value.TotalSeconds);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_IsConsistent()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var expiration1 = query.Expiration;
    var expiration2 = query.Expiration;

    // Assert
    Assert.Equal(expiration1, expiration2);
    Assert.Equal(TimeSpan.FromMinutes(1), expiration1);
    Assert.Equal(TimeSpan.FromMinutes(1), expiration2);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_IsOneMinuteForAllInstances()
  {
    // Arrange
    var query1 = new GetAuthenticationStatisticsQuery();
    var query2 = new GetAuthenticationStatisticsQuery();

    // Act
    var expiration1 = query1.Expiration;
    var expiration2 = query2.Expiration;

    // Assert
    Assert.Equal(expiration1, expiration2);
    Assert.Equal(TimeSpan.FromMinutes(1), expiration1);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_ImplementsICachedQuery()
  {
    // Arrange & Act
    var query = new GetAuthenticationStatisticsQuery();

    // Assert
    Assert.IsAssignableFrom<AppTemplate.Application.Services.Caching.ICachedQuery<GetAuthenticationStatisticsQueryResponse>>(query);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_RecordEquality_WorksCorrectly()
  {
    // Arrange
    var query1 = new GetAuthenticationStatisticsQuery();
    var query2 = new GetAuthenticationStatisticsQuery();

    // Act & Assert
    Assert.Equal(query1, query2);
    Assert.True(query1 == query2);
    Assert.False(query1 != query2);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_GetHashCode_IsConsistent()
  {
    // Arrange
    var query1 = new GetAuthenticationStatisticsQuery();
    var query2 = new GetAuthenticationStatisticsQuery();

    // Act
    var hashCode1 = query1.GetHashCode();
    var hashCode2 = query2.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_ToString_ContainsQueryName()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var stringRepresentation = query.ToString();

    // Assert
    Assert.Contains("GetAuthenticationStatisticsQuery", stringRepresentation);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheKey_DoesNotContainParameters()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal("authentication-statistics", cacheKey);
    Assert.DoesNotContain("{", cacheKey);
    Assert.DoesNotContain("}", cacheKey);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_IsShortDuration()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.True(expiration.Value.TotalSeconds <= 60);
    Assert.True(expiration.Value.TotalSeconds > 0);
  }
}