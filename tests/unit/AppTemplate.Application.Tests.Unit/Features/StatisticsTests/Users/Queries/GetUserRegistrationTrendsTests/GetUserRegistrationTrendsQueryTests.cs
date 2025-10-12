using AppTemplate.Application.Features.Statistics.Users.Queries.GetUserRegistrationTrends;
using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Users.Queries.GetUserRegistrationTrendsTests;

[Trait("Category", "Unit")]
public class GetUserRegistrationTrendsQueryUnitTests
{
  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_IsConstantAcrossInstances()
  {
    // Arrange
    var query1 = new GetUserRegistrationTrendsQuery();
    var query2 = new GetUserRegistrationTrendsQuery();
    var query3 = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKeys = new[] { query1.CacheKey, query2.CacheKey, query3.CacheKey };

    // Assert
    Assert.All(cacheKeys, key => Assert.Equal("users-registration-trends", key));
    Assert.Single(cacheKeys.Distinct());
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_HasFixedDuration()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.Equal(TimeSpan.FromMinutes(5), expiration);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_ImplementsICachedQueryInterface()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var isCachedQuery = query is ICachedQuery<GetUserRegistrationTrendsQueryResponse>;

    // Assert
    Assert.True(isCachedQuery);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKeyFormat_IsValid()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Matches(@"^[a-z\-]+$", cacheKey); // Only lowercase letters and hyphens
    Assert.DoesNotContain(" ", cacheKey);
    Assert.DoesNotContain(":", cacheKey);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_MultipleQueries_ShareSameCacheKey()
  {
    // Arrange
    var queries = Enumerable.Range(0, 10)
        .Select(_ => new GetUserRegistrationTrendsQuery())
        .ToList();

    // Act
    var cacheKeys = queries.Select(q => q.CacheKey).ToList();

    // Assert
    Assert.Equal(10, cacheKeys.Count);
    Assert.Single(cacheKeys.Distinct()); // All have the same cache key
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_IsSuitable_ForStatistics()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    // Statistics should have reasonable cache duration
    Assert.True(expiration.Value <= TimeSpan.FromMinutes(10));
    Assert.True(expiration.Value >= TimeSpan.FromMinutes(1));
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_IsDescriptive()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Contains("users", cacheKey.ToLower());
    Assert.Contains("registration", cacheKey.ToLower());
    Assert.Contains("trends", cacheKey.ToLower());
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_RecordType_SupportsValueEquality()
  {
    // Arrange
    var query1 = new GetUserRegistrationTrendsQuery();
    var query2 = new GetUserRegistrationTrendsQuery();

    // Act
    var areEqual = query1.Equals(query2);
    var hashCodesEqual = query1.GetHashCode() == query2.GetHashCode();

    // Assert
    Assert.True(areEqual);
    Assert.True(hashCodesEqual);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_MatchesIntendedCacheDuration()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();
    var expectedDuration = TimeSpan.FromMinutes(5);

    // Act
    var actualDuration = query.Expiration;

    // Assert
    Assert.Equal(expectedDuration, actualDuration);
    Assert.Equal(300000, actualDuration.Value.TotalMilliseconds); // 5 minutes in milliseconds
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheProperties_AreThreadSafe()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();
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
    Assert.All(results, r => Assert.Equal("users-registration-trends", r.CacheKey));
    Assert.All(results, r => Assert.Equal(TimeSpan.FromMinutes(5), r.Expiration));
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Constructor_CreatesValidInstance()
  {
    // Arrange & Act
    var query = new GetUserRegistrationTrendsQuery();

    // Assert
    Assert.NotNull(query);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_ToString_ContainsTypeName()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var stringRepresentation = query.ToString();

    // Assert
    Assert.Contains("GetUserRegistrationTrendsQuery", stringRepresentation);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_EqualityOperator_WorksCorrectly()
  {
    // Arrange
    var query1 = new GetUserRegistrationTrendsQuery();
    var query2 = new GetUserRegistrationTrendsQuery();

    // Act & Assert
    Assert.True(query1 == query2);
    Assert.False(query1 != query2);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_ComparedWithNull_ReturnsFalse()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var isEqual = query.Equals(null);

    // Assert
    Assert.False(isEqual);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_MultipleInstances_AreAllEqual()
  {
    // Arrange
    var queries = Enumerable.Range(0, 5)
        .Select(_ => new GetUserRegistrationTrendsQuery())
        .ToList();

    // Act
    var allEqual = queries.All(q => q.Equals(queries[0]));
    var distinctCount = queries.Distinct().Count();

    // Assert
    Assert.True(allEqual);
    Assert.Equal(1, distinctCount);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_NeverChanges()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();
    var cacheKey1 = query.CacheKey;

    // Act - Access multiple times
    var cacheKey2 = query.CacheKey;
    var cacheKey3 = query.CacheKey;

    // Assert
    Assert.Equal(cacheKey1, cacheKey2);
    Assert.Equal(cacheKey2, cacheKey3);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_NeverChanges()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();
    var expiration1 = query.Expiration;

    // Act - Access multiple times
    var expiration2 = query.Expiration;
    var expiration3 = query.Expiration;

    // Assert
    Assert.Equal(expiration1, expiration2);
    Assert.Equal(expiration2, expiration3);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_IsNotNullOrEmpty()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.False(string.IsNullOrEmpty(cacheKey));
    Assert.False(string.IsNullOrWhiteSpace(cacheKey));
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_IsPositiveValue()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.True(expiration.Value > TimeSpan.Zero);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_WithExpression_CreatesNewEqualInstance()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var modifiedQuery = query with { };

    // Assert
    Assert.Equal(query, modifiedQuery);
    Assert.Equal(query.CacheKey, modifiedQuery.CacheKey);
    Assert.Equal(query.Expiration, modifiedQuery.Expiration);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_IsLowercase()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal(cacheKey.ToLower(), cacheKey);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_MatchesRoleStatistics()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();
    var roleStatisticsExpiration = TimeSpan.FromMinutes(5);

    // Act
    var trendsExpiration = query.Expiration;

    // Assert
    Assert.NotNull(trendsExpiration);
    Assert.Equal(roleStatisticsExpiration, trendsExpiration.Value);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_DoesNotContainSpaces()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.DoesNotContain(" ", cacheKey);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_GetHashCode_IsConsistent()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();
    var hashCode1 = query.GetHashCode();

    // Act
    var hashCode2 = query.GetHashCode();
    var hashCode3 = query.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
    Assert.Equal(hashCode2, hashCode3);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_HasProperHyphenation()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal("users-registration-trends", cacheKey);
    Assert.Equal(2, cacheKey.Count(c => c == '-')); // Exactly 2 hyphens
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_FollowsNamingConvention()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    // Format: {entity}-{operation}-{descriptor}
    Assert.StartsWith("users-", cacheKey);
    Assert.EndsWith("-trends", cacheKey);
    Assert.Contains("-registration-", cacheKey);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_IsAppropriateForTrendData()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    // Trend data that includes 30 days of history should be cached longer than real-time data
    // but not too long to avoid stale statistics
    Assert.True(expiration.Value >= TimeSpan.FromMinutes(5));
    Assert.True(expiration.Value <= TimeSpan.FromMinutes(15));
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_Length_IsReasonable()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.True(cacheKey.Length > 10); // Not too short
    Assert.True(cacheKey.Length < 100); // Not too long
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_DifferentInstances_ProduceSameHashCode()
  {
    // Arrange
    var query1 = new GetUserRegistrationTrendsQuery();
    var query2 = new GetUserRegistrationTrendsQuery();

    // Act
    var hashCode1 = query1.GetHashCode();
    var hashCode2 = query2.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_CacheKey_DoesNotContainSpecialCharacters()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.DoesNotContain("@", cacheKey);
    Assert.DoesNotContain("#", cacheKey);
    Assert.DoesNotContain("$", cacheKey);
    Assert.DoesNotContain("%", cacheKey);
    Assert.DoesNotContain("&", cacheKey);
    Assert.DoesNotContain("*", cacheKey);
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_InMilliseconds_IsCorrect()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var expirationMs = query.Expiration?.TotalMilliseconds;

    // Assert
    Assert.NotNull(expirationMs);
    Assert.Equal(300000, expirationMs); // 5 minutes = 300,000 milliseconds
  }

  [Fact]
  public void GetUserRegistrationTrendsQuery_Expiration_InSeconds_IsCorrect()
  {
    // Arrange
    var query = new GetUserRegistrationTrendsQuery();

    // Act
    var expirationSeconds = query.Expiration?.TotalSeconds;

    // Assert
    Assert.NotNull(expirationSeconds);
    Assert.Equal(300, expirationSeconds); // 5 minutes = 300 seconds
  }
}