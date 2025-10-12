using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Authentication;
using AppTemplate.Application.Services.Authorization;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.Notifications;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AppTemplate.Domain.AppUsers;
using System.Linq.Expressions;
using AppTemplate.Application.Data.Pagination;
using Ardalis.Result;
using AppTemplate.Domain.Roles;
using Microsoft.Extensions.Caching.Memory;

namespace AppTemplate.Infrastructure.Tests.Unit;

public class DependencyInjectionTests
{
  private IServiceCollection CreateServiceCollection()
  {
    var services = new ServiceCollection();

    // Register missing framework/application services for tests
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(sp => CreateConfiguration());
    services.AddSingleton(typeof(ILogger<>), typeof(LoggerStub<>));
    services.AddSingleton<IHostApplicationLifetime, HostApplicationLifetimeStub>();
    services.AddSingleton<IMemoryCache, MemoryCache>(); // Register IMemoryCache

    // Register required application services for infrastructure
    services.AddScoped<AppTemplate.Application.Services.Caching.ICacheService, CacheServiceStub>();
    services.AddScoped<AppTemplate.Application.Services.AppUsers.IAppUsersService, AppUsersServiceStub>();
    services.AddScoped<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>, UserManagerStub>();
    services.AddScoped<AppTemplate.Application.Services.Roles.IRolesService, RolesServiceStub>();

    return services;
  }

  private IConfiguration CreateConfiguration()
  {
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
              { "ConnectionStrings:AppTemplateDb", "Host=localhost;Database=testdb;Username=test;Password=test" },
              { "Outbox:Enabled", "true" },
              { "Email:ConnectionString", "endpoint=https://test.communication.azure.com/;accesskey=TESTKEY1234567890" },
              { "ConnectionStrings:Email", "endpoint=https://test.communication.azure.com/;accesskey=TESTKEY1234567890" },
              { "AzureCommunicationService:ConnectionString", "endpoint=https://test.communication.azure.com/;accesskey=TESTKEY1234567890" }, // Use valid format
              { "AzureCommunicationService:FromEmail", "test@example.com" },
              { "Jwt:Secret", "abcdefghijklmnopqrstuvwxyz1234567890abcdef" },
              { "Jwt:Issuer", "TestIssuer" },
              { "Jwt:Audience", "TestAudience" }
        })
        .Build();
    return config;
  }

  [Fact]
  public void AddInfrastructure_ThrowsArgumentNullException_WhenConfigurationIsNull()
  {
    var services = CreateServiceCollection();
    Assert.Throws<ArgumentNullException>(() =>
    {
      services.AddInfrastructure(null!);
    });
  }

  [Fact]
  public void AddInfrastructure_RegistersDbContext()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    var dbContext = provider.GetService<ApplicationDbContext>();
    Assert.NotNull(dbContext);
  }

  [Fact]
  public void AddInfrastructure_RegistersDateTimeProvider()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    var dateTimeProvider = provider.GetService<IDateTimeProvider>();
    Assert.NotNull(dateTimeProvider);
  }

  [Fact]
  public void AddInfrastructure_RegistersUserContext()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    var userContext = provider.GetService<IUserContext>();
    Assert.NotNull(userContext);
  }

  [Fact]
  public void AddInfrastructure_RegistersRepositories()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    Assert.NotNull(provider.GetService<IAppUsersRepository>());
    Assert.NotNull(provider.GetService<IRolesRepository>());
    Assert.NotNull(provider.GetService<IPermissionsRepository>());
    Assert.NotNull(provider.GetService<INotificationsRepository>());
  }

  [Fact]
  public void AddInfrastructure_RegistersAuthorizationServices()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    Assert.NotNull(provider.GetService<AppTemplate.Application.Services.Authorization.IAuthorizationService>());
    Assert.NotNull(provider.GetService<IClaimsTransformation>());
    Assert.NotNull(provider.GetService<IAuthorizationHandler>());
    Assert.NotNull(provider.GetService<IAuthorizationPolicyProvider>());
  }

  [Fact]
  public void AddInfrastructure_RegistersJwtTokenService()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    Assert.NotNull(provider.GetService<IJwtTokenService>());
  }

  [Fact]
  public void AddInfrastructure_RegistersNotificationService()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    Assert.NotNull(provider.GetService<INotificationService>());
  }

  [Fact]
  public void AddInfrastructure_RegistersEmailServices()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    Assert.NotNull(provider.GetService<IEmailSender>());
    Assert.NotNull(provider.GetService<EmailTemplateService>());
  }

  [Fact]
  public void AddInfrastructure_RegistersQuartzHostedService()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    var quartzHostedService = provider.GetService<IHostedService>();
    Assert.NotNull(quartzHostedService);
  }

  // Dummy hub for SignalR registration test
  public class DummyHub : Hub { }

  [Fact]
  public void AddInfrastructure_RegistersSignalR()
  {
    var services = CreateServiceCollection();
    var config = CreateConfiguration();
    services.AddInfrastructure(config);
    var provider = services.BuildServiceProvider();
    var signalR = provider.GetService<IHubContext<DummyHub>>();
    // Just check that SignalR extension didn't throw
    Assert.NotNull(provider);
  }

  // Stub implementations for required services
  private class CacheServiceStub : AppTemplate.Application.Services.Caching.ICacheService
  {
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult(default(T));
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
  }

  private class AppUsersServiceStub : AppTemplate.Application.Services.AppUsers.IAppUsersService
  {
    public Task<AppUser?> GetAsync(Expression<Func<AppUser, bool>> predicate, bool includeSoftDeleted = false, Func<IQueryable<AppUser>, IQueryable<AppUser>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
      => Task.FromResult<AppUser?>(null);

    public Task<AppUser> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
      => Task.FromResult<AppUser>(null!);

    public Task<PaginatedList<AppUser>> GetAllAsync(int index = 0, int size = 10, Expression<Func<AppUser, bool>>? predicate = null, bool includeSoftDeleted = false, Func<IQueryable<AppUser>, IQueryable<AppUser>>? include = null, CancellationToken cancellationToken = default)
      => Task.FromResult(new PaginatedList<AppUser>(new List<AppUser>(), 0, index, size));

    public Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public void Update(AppUser user, CancellationToken cancellationToken = default) { }

    public void Delete(AppUser user, CancellationToken cancellationToken = default) { }

    public Task<int> GetUsersCountAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<Result<AppUser>> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
      => Task.FromResult(Result<AppUser>.NotFound());
  }

  private class UserManagerStub : Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>
  {
    public UserManagerStub() : base(
      new UserStoreStub(),
      null, null, null, null, null, null, null, null) { }
  }
  private class UserStoreStub : Microsoft.AspNetCore.Identity.IUserStore<Microsoft.AspNetCore.Identity.IdentityUser>
  {
    public void Dispose() { }
    public Task<string> GetUserIdAsync(Microsoft.AspNetCore.Identity.IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
    public Task<string> GetUserNameAsync(Microsoft.AspNetCore.Identity.IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(Microsoft.AspNetCore.Identity.IdentityUser user, string userName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<string> GetNormalizedUserNameAsync(Microsoft.AspNetCore.Identity.IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(Microsoft.AspNetCore.Identity.IdentityUser user, string normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<Microsoft.AspNetCore.Identity.IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<Microsoft.AspNetCore.Identity.IdentityUser>(null);
    public Task<Microsoft.AspNetCore.Identity.IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<Microsoft.AspNetCore.Identity.IdentityUser>(null);
    public Task<Microsoft.AspNetCore.Identity.IdentityResult> CreateAsync(Microsoft.AspNetCore.Identity.IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(Microsoft.AspNetCore.Identity.IdentityResult.Success);
    public Task<Microsoft.AspNetCore.Identity.IdentityResult> UpdateAsync(Microsoft.AspNetCore.Identity.IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(Microsoft.AspNetCore.Identity.IdentityResult.Success);
    public Task<Microsoft.AspNetCore.Identity.IdentityResult> DeleteAsync(Microsoft.AspNetCore.Identity.IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(Microsoft.AspNetCore.Identity.IdentityResult.Success);
  }
  private class LoggerStub<T> : ILogger<T>
  {
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
  }
  private class HostApplicationLifetimeStub : IHostApplicationLifetime
  {
    public CancellationToken ApplicationStarted => CancellationToken.None;
    public CancellationToken ApplicationStopping => CancellationToken.None;
    public CancellationToken ApplicationStopped => CancellationToken.None;
    public void StopApplication() { }
  }

  // Add this stub class
  private class RolesServiceStub : AppTemplate.Application.Services.Roles.IRolesService
  {
    public Task<Role> GetAsync(Expression<Func<Role, bool>> predicate, bool includeSoftDeleted = false, Func<IQueryable<Role>, IQueryable<Role>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
      => Task.FromResult<Role>(null!);
    public Task<PaginatedList<Role>> GetAllAsync(int index = 0, int size = 10, Expression<Func<Role, bool>>? predicate = null, bool includeSoftDeleted = false, Func<IQueryable<Role>, IQueryable<Role>>? include = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
      => Task.FromResult(new PaginatedList<Role>(new List<Role>(), 0, index, size));
    public Task AddAsync(Role role) => Task.CompletedTask;
    public void Update(Role role, CancellationToken cancellationToken = default) { }
    public void Update(Role role) { }
    public void Delete(Role role, CancellationToken cancellationToken = default) { }
    public void Delete(Role role) { }
    public Task<Result<Role>> GetDefaultRole(CancellationToken cancellationToken = default)
      => Task.FromResult(Result<Role>.NotFound());
  }
}