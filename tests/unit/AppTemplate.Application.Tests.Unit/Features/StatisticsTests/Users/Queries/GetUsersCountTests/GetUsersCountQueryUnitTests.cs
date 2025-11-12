using AppTemplate.Application.Features.Statistics.Users.Queries.GetUsersCount;
using AppTemplate.Application.Services.Caching;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Users.Queries.GetUsersCount;

[Trait("Category", "Unit")]
public class GetUsersCountQueryUnitTests
{
  [Fact]
  public void GetUsersCountQuery_CacheKey_IsConstantAcrossInstances()
  {
    // Arrange
    var query1 = new GetUsersCountQuery();
    var query2 = new GetUsersCountQuery();
    var query3 = new GetUsersCountQuery();

    // Act
    var cacheKeys = new[] { query1.CacheKey, query2.CacheKey, query3.CacheKey };

    // Assert
    Assert.All(cacheKeys, key => Assert.Equal("users-count", key));
    Assert.Single(cacheKeys.Distinct());
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_HasFixedDuration()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.Equal(TimeSpan.FromMinutes(5), expiration);
  }

  [Fact]
  public void GetUsersCountQuery_ImplementsICachedQueryInterface()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var isCachedQuery = query is ICachedQuery<GetUsersCountQueryResponse>;

    // Assert
    Assert.True(isCachedQuery);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKeyFormat_IsValid()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Matches(@"^[a-z\-]+$", cacheKey); // Only lowercase letters and hyphens
    Assert.DoesNotContain(" ", cacheKey);
    Assert.DoesNotContain(":", cacheKey);
  }

  [Fact]
  public void GetUsersCountQuery_MultipleQueries_ShareSameCacheKey()
  {
    // Arrange
    var queries = Enumerable.Range(0, 10)
        .Select(_ => new GetUsersCountQuery())
        .ToList();

    // Act
    var cacheKeys = queries.Select(q => q.CacheKey).ToList();

    // Assert
    Assert.Equal(10, cacheKeys.Count);
    Assert.Single(cacheKeys.Distinct()); // All have the same cache key
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_IsSuitable_ForStatistics()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    // Statistics should have reasonable cache duration
    Assert.True(expiration.Value <= TimeSpan.FromMinutes(10));
    Assert.True(expiration.Value >= TimeSpan.FromMinutes(1));
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_IsDescriptive()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Contains("users", cacheKey.ToLower());
    Assert.Contains("count", cacheKey.ToLower());
  }

  [Fact]
  public void GetUsersCountQuery_RecordType_SupportsValueEquality()
  {
    // Arrange
    var query1 = new GetUsersCountQuery();
    var query2 = new GetUsersCountQuery();

    // Act
    var areEqual = query1.Equals(query2);
    var hashCodesEqual = query1.GetHashCode() == query2.GetHashCode();

    // Assert
    Assert.True(areEqual);
    Assert.True(hashCodesEqual);
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_MatchesIntendedCacheDuration()
  {
    // Arrange
    var query = new GetUsersCountQuery();
    var expectedDuration = TimeSpan.FromMinutes(5);

    // Act
    var actualDuration = query.Expiration;

    // Assert
    Assert.Equal(expectedDuration, actualDuration);
    Assert.Equal(300000, actualDuration.Value.TotalMilliseconds); // 5 minutes in milliseconds
  }

  [Fact]
  public void GetUsersCountQuery_CacheProperties_AreThreadSafe()
  {
    // Arrange
    var query = new GetUsersCountQuery();
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
    Assert.All(results, r => Assert.Equal("users-count", r.CacheKey));
    Assert.All(results, r => Assert.Equal(TimeSpan.FromMinutes(5), r.Expiration));
  }

  [Fact]
  public void GetUsersCountQuery_Constructor_CreatesValidInstance()
  {
    // Arrange & Act
    var query = new GetUsersCountQuery();

    // Assert
    Assert.NotNull(query);
    Assert.NotNull(query.CacheKey);
    Assert.NotNull(query.Expiration);
  }

  [Fact]
  public void GetUsersCountQuery_ToString_ContainsTypeName()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var stringRepresentation = query.ToString();

    // Assert
    Assert.Contains("GetUsersCountQuery", stringRepresentation);
  }

  [Fact]
  public void GetUsersCountQuery_EqualityOperator_WorksCorrectly()
  {
    // Arrange
    var query1 = new GetUsersCountQuery();
    var query2 = new GetUsersCountQuery();

    // Act & Assert
    Assert.True(query1 == query2);
    Assert.False(query1 != query2);
  }

  [Fact]
  public void GetUsersCountQuery_ComparedWithNull_ReturnsFalse()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var isEqual = query.Equals(null);

    // Assert
    Assert.False(isEqual);
  }

  [Fact]
  public void GetUsersCountQuery_MultipleInstances_AreAllEqual()
  {
    // Arrange
    var queries = Enumerable.Range(0, 5)
        .Select(_ => new GetUsersCountQuery())
        .ToList();

    // Act
    var allEqual = queries.All(q => q.Equals(queries[0]));
    var distinctCount = queries.Distinct().Count();

    // Assert
    Assert.True(allEqual);
    Assert.Equal(1, distinctCount);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_NeverChanges()
  {
    // Arrange
    var query = new GetUsersCountQuery();
    var cacheKey1 = query.CacheKey;

    // Act - Access multiple times
    var cacheKey2 = query.CacheKey;
    var cacheKey3 = query.CacheKey;

    // Assert
    Assert.Equal(cacheKey1, cacheKey2);
    Assert.Equal(cacheKey2, cacheKey3);
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_NeverChanges()
  {
    // Arrange
    var query = new GetUsersCountQuery();
    var expiration1 = query.Expiration;

    // Act - Access multiple times
    var expiration2 = query.Expiration;
    var expiration3 = query.Expiration;

    // Assert
    Assert.Equal(expiration1, expiration2);
    Assert.Equal(expiration2, expiration3);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_IsNotNullOrEmpty()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.False(string.IsNullOrEmpty(cacheKey));
    Assert.False(string.IsNullOrWhiteSpace(cacheKey));
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_IsPositiveValue()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.True(expiration.Value > TimeSpan.Zero);
  }

  [Fact]
  public void GetUsersCountQuery_WithExpression_CreatesNewEqualInstance()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var modifiedQuery = query with { };

    // Assert
    Assert.Equal(query, modifiedQuery);
    Assert.Equal(query.CacheKey, modifiedQuery.CacheKey);
    Assert.Equal(query.Expiration, modifiedQuery.Expiration);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_IsLowercase()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal(cacheKey.ToLower(), cacheKey);
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_MatchesOtherUserStatistics()
  {
    // Arrange
    var query = new GetUsersCountQuery();
    var otherStatisticsExpiration = TimeSpan.FromMinutes(5);

    // Act
    var countExpiration = query.Expiration;

    // Assert
    Assert.NotNull(countExpiration);
    Assert.Equal(otherStatisticsExpiration, countExpiration.Value);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_DoesNotContainSpaces()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.DoesNotContain(" ", cacheKey);
  }

  [Fact]
  public void GetUsersCountQuery_GetHashCode_IsConsistent()
  {
    // Arrange
    var query = new GetUsersCountQuery();
    var hashCode1 = query.GetHashCode();

    // Act
    var hashCode2 = query.GetHashCode();
    var hashCode3 = query.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
    Assert.Equal(hashCode2, hashCode3);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_HasProperHyphenation()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.Equal("users-count", cacheKey);
    Assert.Equal(1, cacheKey.Count(c => c == '-')); // Exactly 1 hyphen
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_FollowsNamingConvention()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    // Format: {entity}-{metric}
    Assert.StartsWith("users-", cacheKey);
    Assert.EndsWith("-count", cacheKey);
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_IsAppropriateForCountData()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    // Count data should be cached for a reasonable duration
    // Not too short (to avoid frequent recalculation) and not too long (to avoid stale data)
    Assert.True(expiration.Value >= TimeSpan.FromMinutes(3));
    Assert.True(expiration.Value <= TimeSpan.FromMinutes(10));
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_Length_IsReasonable()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.True(cacheKey.Length > 5); // Not too short
    Assert.True(cacheKey.Length < 50); // Not too long
  }

  [Fact]
  public void GetUsersCountQuery_DifferentInstances_ProduceSameHashCode()
  {
    // Arrange
    var query1 = new GetUsersCountQuery();
    var query2 = new GetUsersCountQuery();

    // Act
    var hashCode1 = query1.GetHashCode();
    var hashCode2 = query2.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_DoesNotContainSpecialCharacters()
  {
    // Arrange
    var query = new GetUsersCountQuery();

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
  public void GetUsersCountQuery_Expiration_InMilliseconds_IsCorrect()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expirationMs = query.Expiration?.TotalMilliseconds;

    // Assert
    Assert.NotNull(expirationMs);
    Assert.Equal(300000, expirationMs); // 5 minutes = 300,000 milliseconds
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_InSeconds_IsCorrect()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expirationSeconds = query.Expiration?.TotalSeconds;

    // Assert
    Assert.NotNull(expirationSeconds);
    Assert.Equal(300, expirationSeconds); // 5 minutes = 300 seconds
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_IsShorterThan_RegistrationTrendsKey()
  {
    // Arrange
    var usersCountQuery = new GetUsersCountQuery();
    var expectedRegistrationTrendsKeyLength = "users-registration-trends".Length;

    // Act
    var cacheKeyLength = usersCountQuery.CacheKey.Length;

    // Assert
    Assert.True(cacheKeyLength < expectedRegistrationTrendsKeyLength);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_IsUnique()
  {
    // Arrange
    var query = new GetUsersCountQuery();
    var otherCacheKeys = new[]
    {
      "users-registration-trends",
      "roles-statistics",
      "authentication-statistics"
    };

    // Act
    var cacheKey = query.CacheKey;

    // Assert
    Assert.DoesNotContain(cacheKey, otherCacheKeys);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_StartsWithEntity()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;
    var parts = cacheKey.Split('-');

    // Assert
    Assert.Equal("users", parts[0]);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_EndsWithMetric()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;
    var parts = cacheKey.Split('-');

    // Assert
    Assert.Equal("count", parts[^1]);
  }

  [Fact]
  public void GetUsersCountQuery_Expiration_IsNotNull()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var expiration = query.Expiration;

    // Assert
    Assert.NotNull(expiration);
    Assert.True(expiration.HasValue);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_DoesNotContainUppercase()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;
    var hasUppercase = cacheKey.Any(char.IsUpper);

    // Assert
    Assert.False(hasUppercase);
  }

  [Fact]
  public void GetUsersCountQuery_CacheKey_OnlyContainsLettersAndHyphens()
  {
    // Arrange
    var query = new GetUsersCountQuery();

    // Act
    var cacheKey = query.CacheKey;
    var isValid = cacheKey.All(c => char.IsLower(c) || c == '-');

    // Assert
    Assert.True(isValid);
  }
}