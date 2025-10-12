using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;
using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Tests.Integration.Features.StatisticsTests.Authentication;

[Trait("Category", "Integration")]
public class GetAuthenticationStatisticsQueryCachingIntegrationTests
{
  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheKey_IsConstantAcrossInstances()
  {
    // Arrange
    var query1 = new GetAuthenticationStatisticsQuery();
    var query2 = new GetAuthenticationStatisticsQuery();
    var query3 = new GetAuthenticationStatisticsQuery();

    // Act
    var cacheKeys = new[] { query1.CacheKey, query2.CacheKey, query3.CacheKey };

    // Assert
    Assert.All(cacheKeys, key => Assert.Equal("authentication-statistics", key));
    Assert.Single(cacheKeys.Distinct());
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_HasFixedDuration()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.Equal(TimeSpan.FromMinutes(1), expiration);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_ImplementsICachedQueryInterface()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var isCachedQuery = query is ICachedQuery<GetAuthenticationStatisticsQueryResponse>;

    // Assert
    Assert.True(isCachedQuery);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheKeyFormat_IsValid()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Matches(@"^[a-z\-]+$", cacheKey); // Only lowercase letters and hyphens
    Assert.DoesNotContain(" ", cacheKey);
    Assert.DoesNotContain(":", cacheKey);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_MultipleQueries_ShareSameCacheKey()
  {
    // Arrange
    var queries = Enumerable.Range(0, 10)
        .Select(_ => new GetAuthenticationStatisticsQuery())
        .ToList();

    // Act
    var cacheKeys = queries.Select(q => q.CacheKey).ToList();

    // Assert
    Assert.Equal(10, cacheKeys.Count);
    Assert.Single(cacheKeys.Distinct()); // All have the same cache key
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_IsSuitable_ForStatistics()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    // Statistics should have short cache duration for fresh data
    Assert.True(expiration.Value <= TimeSpan.FromMinutes(5));
    Assert.True(expiration.Value >= TimeSpan.FromSeconds(30));
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheKey_IsDescriptive()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Contains("authentication", cacheKey.ToLower());
    Assert.Contains("statistics", cacheKey.ToLower());
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_RecordType_SupportsValueEquality()
  {
    // Arrange
    var query1 = new GetAuthenticationStatisticsQuery();
    var query2 = new GetAuthenticationStatisticsQuery();

    // Act
    var areEqual = query1.Equals(query2);
    var hashCodesEqual = query1.GetHashCode() == query2.GetHashCode();

    // Assert
    Assert.True(areEqual);
    Assert.True(hashCodesEqual);
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_Expiration_MatchesIntendedCacheDuration()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();
    var expectedDuration = TimeSpan.FromMinutes(1);

    // Act
    var actualDuration = query.Expiration;

    // Assert
    Assert.Equal(expectedDuration, actualDuration);
    Assert.Equal(60000, actualDuration.Value.TotalMilliseconds); // 1 minute in milliseconds
  }

  [Fact]
  public void GetAuthenticationStatisticsQuery_CacheProperties_AreThreadSafe()
  {
    // Arrange
    var query = new GetAuthenticationStatisticsQuery();
    var results = new List<(string CacheKey, TimeSpan? Expiration)>();
    var lockObject = new object();

    // Act - Access properties from multiple threads
    Parallel.For(0, 100, _ =>
    {
      var cacheKey = query.CacheKey;
      var expiration = query.Expiration;

      lock (lockObject)
      {
        results.Add((cacheKey, expiration));
      }
    });

    // Assert
    Assert.All(results, r => Assert.Equal("authentication-statistics", r.CacheKey));
    Assert.All(results, r => Assert.Equal(TimeSpan.FromMinutes(1), r.Expiration));
  }
}