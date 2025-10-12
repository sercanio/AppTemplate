using AppTemplate.Application.Features.Statistics.Roles.Queries.GetRoleStatistics;
using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Roles.Queries.GetRoleStatisticsTests;

[Trait("Category", "Unit")]
public class GetRoleStatisticsQueryUnitTests
{
  [Fact]
  public void GetRoleStatisticsQuery_CacheKey_IsConstantAcrossInstances()
  {
    // Arrange
    var query1 = new GetRoleStatisticsQuery();
    var query2 = new GetRoleStatisticsQuery();
    var query3 = new GetRoleStatisticsQuery();

    // Act
    var cacheKeys = new[] { query1.CacheKey, query2.CacheKey, query3.CacheKey };

    // Assert
    Assert.All(cacheKeys, key => Assert.Equal("roles-statistics", key));
    Assert.Single(cacheKeys.Distinct());
  }

  [Fact]
  public void GetRoleStatisticsQuery_Expiration_HasFixedDuration()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.Equal(TimeSpan.FromMinutes(5), expiration);
  }

  [Fact]
  public void GetRoleStatisticsQuery_ImplementsICachedQueryInterface()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var isCachedQuery = query is ICachedQuery<GetRoleStatisticsQueryResponse>;

    // Assert
    Assert.True(isCachedQuery);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheKeyFormat_IsValid()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Matches(@"^[a-z\-]+$", cacheKey); // Only lowercase letters and hyphens
    Assert.DoesNotContain(" ", cacheKey);
    Assert.DoesNotContain(":", cacheKey);
  }

  [Fact]
  public void GetRoleStatisticsQuery_MultipleQueries_ShareSameCacheKey()
  {
    // Arrange
    var queries = Enumerable.Range(0, 10)
        .Select(_ => new GetRoleStatisticsQuery())
        .ToList();

    // Act
    var cacheKeys = queries.Select(q => q.CacheKey).ToList();

    // Assert
    Assert.Equal(10, cacheKeys.Count);
    Assert.Single(cacheKeys.Distinct()); // All have the same cache key
  }

  [Fact]
  public void GetRoleStatisticsQuery_Expiration_IsSuitable_ForStatistics()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    // Statistics should have reasonable cache duration
    Assert.True(expiration.Value <= TimeSpan.FromMinutes(10));
    Assert.True(expiration.Value >= TimeSpan.FromMinutes(1));
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheKey_IsDescriptive()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Contains("roles", cacheKey.ToLower());
    Assert.Contains("statistics", cacheKey.ToLower());
  }

  [Fact]
  public void GetRoleStatisticsQuery_RecordType_SupportsValueEquality()
  {
    // Arrange
    var query1 = new GetRoleStatisticsQuery();
    var query2 = new GetRoleStatisticsQuery();

    // Act
    var areEqual = query1.Equals(query2);
    var hashCodesEqual = query1.GetHashCode() == query2.GetHashCode();

    // Assert
    Assert.True(areEqual);
    Assert.True(hashCodesEqual);
  }

  [Fact]
  public void GetRoleStatisticsQuery_Expiration_MatchesIntendedCacheDuration()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();
    var expectedDuration = TimeSpan.FromMinutes(5);

    // Act
    var actualDuration = query.Expiration;

    // Assert
    Assert.Equal(expectedDuration, actualDuration);
    Assert.Equal(300000, actualDuration.Value.TotalMilliseconds); // 5 minutes in milliseconds
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheProperties_AreThreadSafe()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();
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
    Assert.All(results, r => Assert.Equal("roles-statistics", r.CacheKey));
    Assert.All(results, r => Assert.Equal(TimeSpan.FromMinutes(5), r.Expiration));
  }

  [Fact]
  public void GetRoleStatisticsQuery_Constructor_CreatesValidInstance()
  {
    // Arrange & Act
    var query = new GetRoleStatisticsQuery();

    // Assert
    Assert.NotNull(query);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetRoleStatisticsQuery_ToString_ContainsTypeName()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var stringRepresentation = query.ToString();

    // Assert
    Assert.Contains("GetRoleStatisticsQuery", stringRepresentation);
  }

  [Fact]
  public void GetRoleStatisticsQuery_EqualityOperator_WorksCorrectly()
  {
    // Arrange
    var query1 = new GetRoleStatisticsQuery();
    var query2 = new GetRoleStatisticsQuery();

    // Act & Assert
    Assert.True(query1 == query2);
    Assert.False(query1 != query2);
  }

  [Fact]
  public void GetRoleStatisticsQuery_ComparedWithNull_ReturnsFalse()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var isEqual = query.Equals(null);

    // Assert
    Assert.False(isEqual);
  }

  [Fact]
  public void GetRoleStatisticsQuery_MultipleInstances_AreAllEqual()
  {
    // Arrange
    var queries = Enumerable.Range(0, 5)
        .Select(_ => new GetRoleStatisticsQuery())
        .ToList();

    // Act
    var allEqual = queries.All(q => q.Equals(queries[0]));
    var distinctCount = queries.Distinct().Count();

    // Assert
    Assert.True(allEqual);
    Assert.Equal(1, distinctCount);
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheKey_NeverChanges()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();
    var cacheKey1 = query.CacheKey;

    // Act - Access multiple times
    var cacheKey2 = query.CacheKey;
    var cacheKey3 = query.CacheKey;

    // Assert
    Assert.Equal(cacheKey1, cacheKey2);
    Assert.Equal(cacheKey2, cacheKey3);
  }

  [Fact]
  public void GetRoleStatisticsQuery_Expiration_NeverChanges()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();
    var expiration1 = query.Expiration;

    // Act - Access multiple times
    var expiration2 = query.Expiration;
    var expiration3 = query.Expiration;

    // Assert
    Assert.Equal(expiration1, expiration2);
    Assert.Equal(expiration2, expiration3);
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheKey_IsNotNullOrEmpty()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.False(string.IsNullOrEmpty(cacheKey));
    Assert.False(string.IsNullOrWhiteSpace(cacheKey));
  }

  [Fact]
  public void GetRoleStatisticsQuery_Expiration_IsPositiveValue()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.True(expiration.Value > TimeSpan.Zero);
  }

  [Fact]
  public void GetRoleStatisticsQuery_WithExpression_CreatesNewEqualInstance()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var modifiedQuery = query with { };

    // Assert
    Assert.Equal(query, modifiedQuery);
    Assert.Equal(query.CacheKey, modifiedQuery.CacheKey);
    Assert.Equal(query.Expiration, modifiedQuery.Expiration);
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheKey_IsLowercase()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal(cacheKey.ToLower(), cacheKey);
  }

  [Fact]
  public void GetRoleStatisticsQuery_Expiration_IsLongerThan_AuthenticationStatistics()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();
    var authStatisticsExpiration = TimeSpan.FromMinutes(1);

    // Act
    var rolesExpiration = query.Expiration;

    // Assert
    Assert.NotNull(rolesExpiration);
    Assert.True(rolesExpiration.Value > authStatisticsExpiration);
  }

  [Fact]
  public void GetRoleStatisticsQuery_CacheKey_DoesNotContainSpaces()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.DoesNotContain(" ", cacheKey);
  }

  [Fact]
  public void GetRoleStatisticsQuery_GetHashCode_IsConsistent()
  {
    // Arrange
    var query = new GetRoleStatisticsQuery();
    var hashCode1 = query.GetHashCode();

    // Act
    var hashCode2 = query.GetHashCode();
    var hashCode3 = query.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
    Assert.Equal(hashCode2, hashCode3);
  }
}