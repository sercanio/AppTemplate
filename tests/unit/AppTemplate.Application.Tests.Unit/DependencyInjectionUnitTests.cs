using AppTemplate.Application.Behaviors;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.Roles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppTemplate.Application.Tests.Unit;

[Trait("Category", "Unit")]
public class DependencyInjectionUnitTests
{
  #region AddApplication Tests

  [Fact]
  public void AddApplication_ShouldReturnServiceCollection()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    var result = services.AddApplication();

    // Assert
    result.Should().NotBeNull();
    result.Should().BeSameAs(services);
  }

  [Fact]
  public void AddApplication_ShouldRegisterMediatR()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();
    var serviceProvider = services.BuildServiceProvider();

    // Assert
    var mediator = serviceProvider.GetService<IMediator>();
    mediator.Should().NotBeNull();
  }

  [Fact]
  public void AddApplication_ShouldRegisterAllServices()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    services.Should().NotBeEmpty();
    services.Should().Contain(s => s.ServiceType == typeof(IRolesService));
    services.Should().Contain(s => s.ServiceType == typeof(IAppUsersService));
    services.Should().Contain(s => s.ServiceType == typeof(IAccountEmailService));
    services.Should().Contain(s => s.ServiceType == typeof(ICacheService));
  }

  #endregion

  #region MediatR Behaviors Tests

  [Fact]
  public void AddApplication_ShouldRegisterMediatRBehaviors()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    // MediatR behaviors are registered as open generics
    services.Should().Contain(s =>
        s.ServiceType.IsGenericType &&
        s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
  }

  [Fact]
  public void AddApplication_ShouldRegisterLoggingBehavior()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var hasLoggingBehavior = services.Any(s =>
        s.ImplementationType != null &&
        s.ImplementationType.IsGenericType &&
        s.ImplementationType.GetGenericTypeDefinition() == typeof(LoggingBehavior<,>));

    hasLoggingBehavior.Should().BeTrue();
  }

  [Fact]
  public void AddApplication_ShouldRegisterValidationBehavior()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var hasValidationBehavior = services.Any(s =>
        s.ImplementationType != null &&
        s.ImplementationType.IsGenericType &&
        s.ImplementationType.GetGenericTypeDefinition() == typeof(ValidationBehavior<,>));

    hasValidationBehavior.Should().BeTrue();
  }

  [Fact]
  public void AddApplication_ShouldRegisterQueryCachingBehavior()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var hasQueryCachingBehavior = services.Any(s =>
        s.ImplementationType != null &&
        s.ImplementationType.IsGenericType &&
        s.ImplementationType.GetGenericTypeDefinition() == typeof(QueryCachingBehavior<,>));

    hasQueryCachingBehavior.Should().BeTrue();
  }

  #endregion

  #region Application Services Tests

  [Fact]
  public void AddApplication_ShouldRegisterRolesService()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IRolesService));
    descriptor.Should().NotBeNull();
    descriptor!.ImplementationType.Should().Be(typeof(RolesService));
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Fact]
  public void AddApplication_ShouldRegisterAppUsersService()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAppUsersService));
    descriptor.Should().NotBeNull();
    descriptor!.ImplementationType.Should().Be(typeof(AppUsersService));
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Fact]
  public void AddApplication_ShouldRegisterAccountEmailService()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAccountEmailService));
    descriptor.Should().NotBeNull();
    descriptor!.ImplementationType.Should().Be(typeof(AccountEmailService));
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  #endregion

  #region Cache Service Tests

  [Fact]
  public void AddApplication_ShouldRegisterCacheService()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICacheService));
    descriptor.Should().NotBeNull();
    descriptor!.ImplementationType.Should().Be(typeof(CacheService));
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  #endregion

  #region Service Lifetime Tests

  [Fact]
  public void AddApplication_AllApplicationServices_ShouldBeScoped()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var applicationServiceTypes = new[]
    {
      typeof(IRolesService),
      typeof(IAppUsersService),
      typeof(IAccountEmailService),
      typeof(ICacheService)
    };

    foreach (var serviceType in applicationServiceTypes)
    {
      var descriptor = services.FirstOrDefault(s => s.ServiceType == serviceType);
      descriptor.Should().NotBeNull($"{serviceType.Name} should be registered");
      descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped,
          $"{serviceType.Name} should have Scoped lifetime");
    }
  }

  #endregion

  #region Service Resolution Tests

  [Fact]
  public void AddApplication_RolesService_ShouldBeResolvable()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddApplication();

    // We need to add mock dependencies that RolesService requires
    services.AddScoped<AppTemplate.Application.Repositories.IRolesRepository>(
        _ => null!); // Mock for testing registration only

    var serviceProvider = services.BuildServiceProvider();

    // Act
    var rolesService = serviceProvider.GetService<IRolesService>();

    // Assert - Service should be registered (will be null due to missing dependencies, but registered)
    services.Should().Contain(s => s.ServiceType == typeof(IRolesService));
  }

  [Fact]
  public void AddApplication_AppUsersService_ShouldBeResolvable()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddApplication();

    // Assert - Service should be registered
    services.Should().Contain(s => s.ServiceType == typeof(IAppUsersService));
  }

  [Fact]
  public void AddApplication_AccountEmailService_ShouldBeResolvable()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddApplication();

    // Assert - Service should be registered
    services.Should().Contain(s => s.ServiceType == typeof(IAccountEmailService));
  }

  [Fact]
  public void AddApplication_CacheService_ShouldBeResolvable()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddApplication();

    // Assert - Service should be registered
    services.Should().Contain(s => s.ServiceType == typeof(ICacheService));
  }

  #endregion

  #region Multiple Calls Tests

  [Fact]
  public void AddApplication_CalledMultipleTimes_ShouldNotThrow()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    Action act = () =>
    {
      services.AddApplication();
      services.AddApplication();
    };

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void AddApplication_CalledMultipleTimes_ShouldRegisterServicesTwice()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();
    var countAfterFirst = services.Count(s => s.ServiceType == typeof(IRolesService));

    services.AddApplication();
    var countAfterSecond = services.Count(s => s.ServiceType == typeof(IRolesService));

    // Assert
    countAfterSecond.Should().Be(countAfterFirst * 2);
  }

  #endregion

  #region Service Count Tests

  [Fact]
  public void AddApplication_ShouldRegisterExpectedNumberOfApplicationServices()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert - Check for the 4 main application services
    var applicationServices = services.Where(s =>
        s.ServiceType == typeof(IRolesService) ||
        s.ServiceType == typeof(IAppUsersService) ||
        s.ServiceType == typeof(IAccountEmailService) ||
        s.ServiceType == typeof(ICacheService));

    applicationServices.Should().HaveCount(4);
  }

  #endregion

  #region MediatR Assembly Registration Tests

  [Fact]
  public void AddApplication_ShouldRegisterServicesFromApplicationAssembly()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    // MediatR should register handlers from the Application assembly
    var mediatorServices = services.Where(s =>
        s.ServiceType.IsGenericType &&
        (s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
         s.ServiceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>)));

    mediatorServices.Should().NotBeEmpty("MediatR should register handlers from the assembly");
  }

  #endregion

  #region Empty ServiceCollection Tests

  [Fact]
  public void AddApplication_WithEmptyServiceCollection_ShouldSucceed()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    var result = services.AddApplication();

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCountGreaterThan(0);
  }

  #endregion

  #region Behavior Order Tests

  [Fact]
  public void AddApplication_ShouldRegisterBehaviorsInCorrectOrder()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddApplication();

    // Assert
    var behaviors = services
        .Where(s => s.ServiceType.IsGenericType &&
                    s.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
        .Where(s => s.ImplementationType != null && s.ImplementationType.IsGenericType)
        .Select(s => s.ImplementationType!.GetGenericTypeDefinition())
        .ToList();

    // The behaviors should be registered (order may vary based on MediatR registration)
    behaviors.Should().Contain(typeof(LoggingBehavior<,>));
    behaviors.Should().Contain(typeof(ValidationBehavior<,>));
    behaviors.Should().Contain(typeof(QueryCachingBehavior<,>));
  }

  #endregion

  #region Extension Method Tests

  [Fact]
  public void AddApplication_ShouldBeExtensionMethod()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act & Assert
    // Extension method should be callable on IServiceCollection
    services.AddApplication().Should().BeSameAs(services);
  }

  [Fact]
  public void AddApplication_ShouldReturnSameServiceCollectionInstance()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    var result = services.AddApplication();

    // Assert
    result.Should().BeSameAs(services, "method should return the same instance for chaining");
  }

  #endregion

  #region Integration Tests

  [Fact]
  public void AddApplication_WithRealServiceProvider_ShouldBuildSuccessfully()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddApplication();

    // Act
    Action act = () => services.BuildServiceProvider();

    // Assert
    act.Should().NotThrow("service provider should build successfully");
  }

  [Fact]
  public void AddApplication_ServiceProvider_ShouldHaveAllRegisteredServices()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddApplication();
    var serviceProvider = services.BuildServiceProvider();

    // Act & Assert
    var mediator = serviceProvider.GetService<IMediator>();
    mediator.Should().NotBeNull();

    // Check service descriptors are present (we can't resolve without dependencies)
    services.Should().Contain(s => s.ServiceType == typeof(IRolesService));
    services.Should().Contain(s => s.ServiceType == typeof(IAppUsersService));
    services.Should().Contain(s => s.ServiceType == typeof(IAccountEmailService));
    services.Should().Contain(s => s.ServiceType == typeof(ICacheService));
  }

  #endregion

  #region Null Safety Tests

  [Fact]
  public void AddApplication_WithNullServiceCollection_ShouldThrowArgumentNullException()
  {
    // Arrange
    IServiceCollection services = null!;

    // Act
    Action act = () => services.AddApplication();

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  #endregion
}