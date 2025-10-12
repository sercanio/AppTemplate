using AppTemplate.Application.Features.Notifications.Queries.GetAllNotifications;

namespace AppTemplate.Application.Tests.Unit.Features.NotificationsTests.Queries.GetAllNotificationsTests;

[Trait("Category", "Unit")]
public class GetAllNotificationsQueryUnitTests
{
  [Fact]
  public void GetAllNotificationsQuery_Constructor_SetsPropertiesCorrectly()
  {
    // Arrange
    var pageIndex = 1;
    var pageSize = 20;
    var cancellationToken = new CancellationToken();

    // Act
    var query = new GetAllNotificationsQuery(pageIndex, pageSize, cancellationToken);

    // Assert
    Assert.Equal(pageIndex, query.PageIndex);
    Assert.Equal(pageSize, query.PageSize);
    Assert.Equal(cancellationToken, query.CancellationToken);
  }

  [Fact]
  public void GetAllNotificationsQuery_RecordType_SupportsValueEquality()
  {
    // Arrange
    var pageIndex = 1;
    var pageSize = 10;
    var cancellationToken = new CancellationToken();

    var query1 = new GetAllNotificationsQuery(pageIndex, pageSize, cancellationToken);
    var query2 = new GetAllNotificationsQuery(pageIndex, pageSize, cancellationToken);

    // Act
    var areEqual = query1.Equals(query2);
    var hashCodesEqual = query1.GetHashCode() == query2.GetHashCode();

    // Assert
    Assert.True(areEqual);
    Assert.True(hashCodesEqual);
  }

  [Fact]
  public void GetAllNotificationsQuery_RecordType_SupportsValueInequality()
  {
    // Arrange
    var query1 = new GetAllNotificationsQuery(0, 10, default);
    var query2 = new GetAllNotificationsQuery(1, 10, default);

    // Act
    var areEqual = query1.Equals(query2);

    // Assert
    Assert.False(areEqual);
  }

  [Fact]
  public void GetAllNotificationsQuery_DifferentPageSize_AreNotEqual()
  {
    // Arrange
    var query1 = new GetAllNotificationsQuery(0, 10, default);
    var query2 = new GetAllNotificationsQuery(0, 20, default);

    // Act
    var areEqual = query1.Equals(query2);

    // Assert
    Assert.False(areEqual);
  }

  [Fact]
  public void GetAllNotificationsQuery_WithDeconstruction_ExtractsPropertiesCorrectly()
  {
    // Arrange
    var expectedPageIndex = 2;
    var expectedPageSize = 50;
    var expectedCancellationToken = new CancellationToken();
    var query = new GetAllNotificationsQuery(expectedPageIndex, expectedPageSize, expectedCancellationToken);

    // Act
    var (pageIndex, pageSize, cancellationToken) = query;

    // Assert
    Assert.Equal(expectedPageIndex, pageIndex);
    Assert.Equal(expectedPageSize, pageSize);
    Assert.Equal(expectedCancellationToken, cancellationToken);
  }

  [Fact]
  public void GetAllNotificationsQuery_WithExpression_CanBeUsedInWith()
  {
    // Arrange
    var query = new GetAllNotificationsQuery(0, 10, default);

    // Act
    var modifiedQuery = query with { PageIndex = 2 };

    // Assert
    Assert.Equal(2, modifiedQuery.PageIndex);
    Assert.Equal(10, modifiedQuery.PageSize);
    Assert.NotEqual(query, modifiedQuery);
  }

  [Theory]
  [InlineData(0, 10)]
  [InlineData(1, 20)]
  [InlineData(5, 50)]
  [InlineData(10, 100)]
  public void GetAllNotificationsQuery_WithVariousPaginationValues_CreatesValidInstances(int pageIndex, int pageSize)
  {
    // Arrange & Act
    var query = new GetAllNotificationsQuery(pageIndex, pageSize, default);

    // Assert
    Assert.Equal(pageIndex, query.PageIndex);
    Assert.Equal(pageSize, query.PageSize);
  }

  [Fact]
  public void GetAllNotificationsQuery_DefaultCancellationToken_IsDefault()
  {
    // Arrange & Act
    var query = new GetAllNotificationsQuery(0, 10, default);

    // Assert
    Assert.Equal(default, query.CancellationToken);
    Assert.False(query.CancellationToken.IsCancellationRequested);
  }

  [Fact]
  public void GetAllNotificationsQuery_WithCancelledToken_PreservesCancellationState()
  {
    // Arrange
    var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act
    var query = new GetAllNotificationsQuery(0, 10, cts.Token);

    // Assert
    Assert.True(query.CancellationToken.IsCancellationRequested);
  }

  [Fact]
  public void GetAllNotificationsQuery_ToString_ContainsPropertyValues()
  {
    // Arrange
    var query = new GetAllNotificationsQuery(3, 25, default);

    // Act
    var stringRepresentation = query.ToString();

    // Assert
    Assert.Contains("PageIndex", stringRepresentation);
    Assert.Contains("PageSize", stringRepresentation);
    Assert.Contains("3", stringRepresentation);
    Assert.Contains("25", stringRepresentation);
  }

  [Fact]
  public void GetAllNotificationsQuery_MultipleInstances_WithSameValues_AreEqual()
  {
    // Arrange
    var queries = Enumerable.Range(0, 5)
        .Select(_ => new GetAllNotificationsQuery(1, 15, default))
        .ToList();

    // Act
    var allEqual = queries.All(q => q.Equals(queries[0]));
    var distinctCount = queries.Distinct().Count();

    // Assert
    Assert.True(allEqual);
    Assert.Equal(1, distinctCount);
  }

  [Fact]
  public void GetAllNotificationsQuery_ImplementsIQuery_Interface()
  {
    // Arrange
    var query = new GetAllNotificationsQuery(0, 10, default);

    // Act
    var implementsIQuery = query is AppTemplate.Application.Services.Messages.IQuery<GetAllNotificationsWithUnreadCountResponse>;

    // Assert
    Assert.True(implementsIQuery);
  }

  [Fact]
  public void GetAllNotificationsQuery_NegativePageIndex_IsAllowed()
  {
    // Arrange & Act
    var query = new GetAllNotificationsQuery(-1, 10, default);

    // Assert
    Assert.Equal(-1, query.PageIndex);
  }

  [Fact]
  public void GetAllNotificationsQuery_ZeroPageSize_IsAllowed()
  {
    // Arrange & Act
    var query = new GetAllNotificationsQuery(0, 0, default);

    // Assert
    Assert.Equal(0, query.PageSize);
  }

  [Fact]
  public void GetAllNotificationsQuery_PropertiesAreThreadSafe()
  {
    // Arrange
    var query = new GetAllNotificationsQuery(5, 30, default);
    var results = new List<(int PageIndex, int PageSize)>();
    var lockObject = new object();

    // Act - Access properties from multiple threads
    Parallel.For(0, 100, _ =>
    {
      var pageIndex = query.PageIndex;
      var pageSize = query.PageSize;

      lock (lockObject)
      {
        results.Add((pageIndex, pageSize));
      }
    });

    // Assert
    Assert.All(results, r => Assert.Equal(5, r.PageIndex));
    Assert.All(results, r => Assert.Equal(30, r.PageSize));
  }

  [Fact]
  public void GetAllNotificationsQuery_EqualityOperator_WorksCorrectly()
  {
    // Arrange
    var query1 = new GetAllNotificationsQuery(2, 15, default);
    var query2 = new GetAllNotificationsQuery(2, 15, default);
    var query3 = new GetAllNotificationsQuery(3, 15, default);

    // Act & Assert
    Assert.True(query1 == query2);
    Assert.False(query1 == query3);
    Assert.False(query1 != query2);
    Assert.True(query1 != query3);
  }

  [Fact]
  public void GetAllNotificationsQuery_ComparedWithNull_ReturnsFalse()
  {
    // Arrange
    var query = new GetAllNotificationsQuery(0, 10, default);

    // Act
    var isEqual = query.Equals(null);

    // Assert
    Assert.False(isEqual);
  }
}