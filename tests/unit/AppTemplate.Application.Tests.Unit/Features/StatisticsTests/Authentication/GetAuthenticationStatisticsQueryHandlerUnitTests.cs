using System.Linq.Expressions;
using AppTemplate.Application.Features.Statistics.Authentication.Queries.GetAuthenticationStatistics;
using AppTemplate.Application.Services.Statistics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace AppTemplate.Application.Tests.Unit.Features.StatisticsTests.Authentication;

[Trait("Category", "Unit")]
public class GetAuthenticationStatisticsQueryHandlerUnitTests
{
  private readonly Mock<IActiveSessionService> _sessionServiceMock = new();
  private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
  private readonly GetAuthenticationStatisticsQueryHandler _handler;

  public GetAuthenticationStatisticsQueryHandlerUnitTests()
  {
    var store = new Mock<IUserStore<IdentityUser>>();
    _userManagerMock = new Mock<UserManager<IdentityUser>>(
        store.Object, null, null, null, null, null, null, null, null);

    _handler = new GetAuthenticationStatisticsQueryHandler(_sessionServiceMock.Object, _userManagerMock.Object);
  }

  [Fact]
  public async Task Handle_ReturnsCorrectStatistics_WhenUsersExist()
  {
    // Arrange
    _sessionServiceMock.Setup(s => s.GetActiveSessionsCountAsync()).ReturnsAsync(5);

    var users = new List<IdentityUser>
        {
            new IdentityUser { Id = "1", UserName = "user1", Email="user1@example.com", TwoFactorEnabled = true },
            new IdentityUser { Id = "2", UserName = "user2", Email="user1@example.com", TwoFactorEnabled = false },
            new IdentityUser { Id = "3", UserName = "user3", Email="user1@example.com", TwoFactorEnabled = true }
        };

    var usersQueryable = users.AsQueryable().BuildMockDbSet();

    _userManagerMock.SetupGet(x => x.Users).Returns(usersQueryable.Object);

    _userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(It.Is<IdentityUser>(u => u.UserName == "user1")))
        .ReturnsAsync("key1");
    _userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(It.Is<IdentityUser>(u => u.UserName == "user2")))
        .ReturnsAsync((string)null);
    _userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(It.Is<IdentityUser>(u => u.UserName == "user3")))
        .ReturnsAsync("key3");

    var query = new GetAuthenticationStatisticsQuery();

    // Act
    var result = await _handler.Handle(query, default);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(5, result.Value.ActiveSessions);
    Assert.Equal(0, result.Value.SuccessfulLogins);
    Assert.Equal(0, result.Value.FailedLogins);
    Assert.Equal(2, result.Value.TwoFactorEnabled);
    Assert.Equal(2, result.Value.TotalUsersWithAuthenticator);
  }

  [Fact]
  public async Task Handle_ReturnsZero_WhenNoUsersExist()
  {
    _sessionServiceMock.Setup(s => s.GetActiveSessionsCountAsync()).ReturnsAsync(0);

    var users = new List<IdentityUser>();
    var usersQueryable = users.AsQueryable().BuildMockDbSet();

    _userManagerMock.SetupGet(x => x.Users).Returns(usersQueryable.Object);

    var query = new GetAuthenticationStatisticsQuery();

    var result = await _handler.Handle(query, default);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(0, result.Value.ActiveSessions);
    Assert.Equal(0, result.Value.SuccessfulLogins);
    Assert.Equal(0, result.Value.FailedLogins);
    Assert.Equal(0, result.Value.TwoFactorEnabled);
    Assert.Equal(0, result.Value.TotalUsersWithAuthenticator);
  }
}

// Helper for mocking IQueryable<IdentityUser> as DbSet<IdentityUser> with async support
public static class QueryableMockExtensions
{
  public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
  {
    var mockSet = new Mock<DbSet<T>>();

    // Setup IQueryable interface
    mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(source.Provider));
    mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
    mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
    mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => source.GetEnumerator());

    // Setup IAsyncEnumerable interface
    mockSet.As<IAsyncEnumerable<T>>()
        .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
        .Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));

    return mockSet;
  }
}

// Test implementations for async query support
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
  private readonly IQueryProvider _inner;

  internal TestAsyncQueryProvider(IQueryProvider inner)
  {
    _inner = inner;
  }

  public IQueryable CreateQuery(Expression expression)
  {
    return new TestAsyncEnumerable<TEntity>(expression);
  }

  public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
  {
    return new TestAsyncEnumerable<TElement>(expression);
  }

  public object Execute(Expression expression)
  {
    return _inner.Execute(expression);
  }

  public TResult Execute<TResult>(Expression expression)
  {
    return _inner.Execute<TResult>(expression);
  }

  public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
  {
    var expectedResultType = typeof(TResult).GetGenericArguments()[0];
    var executionResult = ((IQueryProvider)this).Execute(expression);

    return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
        ?.MakeGenericMethod(expectedResultType)
        ?.Invoke(null, new[] { executionResult });
  }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
  public TestAsyncEnumerable(IEnumerable<T> enumerable)
      : base(enumerable)
  { }

  public TestAsyncEnumerable(Expression expression)
      : base(expression)
  { }

  public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
  {
    return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
  }

  IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
  private readonly IEnumerator<T> _inner;

  public TestAsyncEnumerator(IEnumerator<T> inner)
  {
    _inner = inner;
  }

  public ValueTask DisposeAsync()
  {
    _inner.Dispose();
    return ValueTask.CompletedTask;
  }

  public ValueTask<bool> MoveNextAsync()
  {
    return ValueTask.FromResult(_inner.MoveNext());
  }

  public T Current => _inner.Current;
}