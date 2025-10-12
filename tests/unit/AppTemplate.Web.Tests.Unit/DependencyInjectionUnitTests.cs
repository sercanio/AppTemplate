using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Application.Services.Localization;
using AppTemplate.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppTemplate.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DependencyInjectionUnitTests
{
  [Fact]
  public void AddWebApi_ShouldReturnServiceCollection()
  {
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();

    // Act
    var result = services.AddWebApi(configuration);

    // Assert
    Assert.NotNull(result);
    Assert.Same(services, result);
  }

  [Fact]
  public void AddWebApi_ShouldRegisterIErrorHandlingServiceAsScoped()
  {
    // Arrange
    var services = new ServiceCollection();
    AddRequiredDependencies(services);
    var configuration = new ConfigurationBuilder().Build();

    // Act
    services.AddWebApi(configuration);

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IErrorHandlingService));

    Assert.NotNull(serviceDescriptor);
    Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    Assert.Equal(typeof(ErrorHandlingService), serviceDescriptor.ImplementationType);

    // Verify the service can be resolved
    var errorHandlingService = serviceProvider.GetService<IErrorHandlingService>();
    Assert.NotNull(errorHandlingService);
    Assert.IsType<ErrorHandlingService>(errorHandlingService);
  }

  [Fact]
  public void AddWebApi_ShouldRegisterILocalizationServiceAsSingleton()
  {
    // Arrange
    var services = new ServiceCollection();
    AddRequiredDependencies(services);
    var configuration = new ConfigurationBuilder().Build();

    // Act
    services.AddWebApi(configuration);

    // Assert
    var serviceProvider = services.BuildServiceProvider();
    var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ILocalizationService));

    Assert.NotNull(serviceDescriptor);
    Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    Assert.Equal(typeof(LocalizationService), serviceDescriptor.ImplementationType);

    // Verify the service can be resolved
    var localizationService = serviceProvider.GetService<ILocalizationService>();
    Assert.NotNull(localizationService);
    Assert.IsType<LocalizationService>(localizationService);
  }

  [Fact]
  public void AddWebApi_ShouldRegisterCorrectNumberOfServices()
  {
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();
    var initialServiceCount = services.Count;

    // Act
    services.AddWebApi(configuration);

    // Assert
    var addedServicesCount = services.Count - initialServiceCount;
    Assert.Equal(2, addedServicesCount); // IErrorHandlingService and ILocalizationService
  }

  [Fact]
  public void AddWebApi_ShouldEnsureScopedServicesHaveDifferentInstancesPerScope()
  {
    // Arrange
    var services = new ServiceCollection();
    AddRequiredDependencies(services);
    var configuration = new ConfigurationBuilder().Build();
    services.AddWebApi(configuration);
    var serviceProvider = services.BuildServiceProvider();

    IErrorHandlingService errorHandlingService1a, errorHandlingService1b;
    IErrorHandlingService errorHandlingService2a, errorHandlingService2b;

    // Act & Assert - Scoped services should be different between scopes but same within scope
    using (var scope1 = serviceProvider.CreateScope())
    {
      errorHandlingService1a = scope1.ServiceProvider.GetRequiredService<IErrorHandlingService>();
      errorHandlingService1b = scope1.ServiceProvider.GetRequiredService<IErrorHandlingService>();

      // Same within scope
      Assert.Same(errorHandlingService1a, errorHandlingService1b);
    }

    using (var scope2 = serviceProvider.CreateScope())
    {
      errorHandlingService2a = scope2.ServiceProvider.GetRequiredService<IErrorHandlingService>();
      errorHandlingService2b = scope2.ServiceProvider.GetRequiredService<IErrorHandlingService>();

      // Same within scope
      Assert.Same(errorHandlingService2a, errorHandlingService2b);
    }

    // Different instances between different scopes
    Assert.NotSame(errorHandlingService1a, errorHandlingService2a);
    Assert.NotSame(errorHandlingService1b, errorHandlingService2b);
  }

  [Fact]
  public void AddWebApi_ShouldEnsureSingletonServicesHaveSameInstanceAlways()
  {
    // Arrange
    var services = new ServiceCollection();
    AddRequiredDependencies(services);
    var configuration = new ConfigurationBuilder().Build();
    services.AddWebApi(configuration);
    var serviceProvider = services.BuildServiceProvider();

    // Act & Assert - Singleton services should always be the same instance
    var localizationService1 = serviceProvider.GetService<ILocalizationService>();
    var localizationService2 = serviceProvider.GetService<ILocalizationService>();

    Assert.Same(localizationService1, localizationService2);

    // Even across different scopes
    using (var scope1 = serviceProvider.CreateScope())
    {
      var scopedLocalizationService1 = scope1.ServiceProvider.GetService<ILocalizationService>();
      Assert.Same(localizationService1, scopedLocalizationService1);
    }

    using (var scope2 = serviceProvider.CreateScope())
    {
      var scopedLocalizationService2 = scope2.ServiceProvider.GetService<ILocalizationService>();
      Assert.Same(localizationService1, scopedLocalizationService2);
    }
  }

  [Fact]
  public void AddWebApi_WithNullConfiguration_ShouldStillRegisterServices()
  {
    // Arrange
    var services = new ServiceCollection();
    AddRequiredDependencies(services);
    IConfiguration configuration = null;

    // Act
    services.AddWebApi(configuration);

    // Assert
    var serviceProvider = services.BuildServiceProvider();

    var errorHandlingService = serviceProvider.GetService<IErrorHandlingService>();
    var localizationService = serviceProvider.GetService<ILocalizationService>();

    Assert.NotNull(errorHandlingService);
    Assert.NotNull(localizationService);
  }

  [Fact]
  public void AddWebApi_ShouldAllowMultipleCalls()
  {
    // Arrange
    var services = new ServiceCollection();
    AddRequiredDependencies(services);
    var configuration = new ConfigurationBuilder().Build();

    // Act
    services.AddWebApi(configuration);
    services.AddWebApi(configuration); // Call again

    // Assert
    var serviceProvider = services.BuildServiceProvider();

    // Services should still be resolvable (last registration wins)
    var errorHandlingService = serviceProvider.GetService<IErrorHandlingService>();
    var localizationService = serviceProvider.GetService<ILocalizationService>();

    Assert.NotNull(errorHandlingService);
    Assert.NotNull(localizationService);

    // Verify we have multiple registrations
    var errorHandlingDescriptors = services.Where(s => s.ServiceType == typeof(IErrorHandlingService)).ToList();
    var localizationDescriptors = services.Where(s => s.ServiceType == typeof(ILocalizationService)).ToList();

    Assert.Equal(2, errorHandlingDescriptors.Count);
    Assert.Equal(2, localizationDescriptors.Count);
  }

  [Fact]
  public void AddWebApi_ShouldRegisterServicesWithCorrectImplementationTypes()
  {
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();

    // Act
    services.AddWebApi(configuration);

    // Assert
    var errorHandlingDescriptor = services.First(s => s.ServiceType == typeof(IErrorHandlingService));
    var localizationDescriptor = services.First(s => s.ServiceType == typeof(ILocalizationService));

    Assert.Equal(typeof(ErrorHandlingService), errorHandlingDescriptor.ImplementationType);
    Assert.Equal(typeof(LocalizationService), localizationDescriptor.ImplementationType);
  }

  [Fact]
  public void AddWebApi_ShouldNotRegisterConcreteTypesDirectly()
  {
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();

    // Act
    services.AddWebApi(configuration);

    // Assert
    var concreteErrorHandling = services.FirstOrDefault(s => s.ServiceType == typeof(ErrorHandlingService));
    var concreteLocalization = services.FirstOrDefault(s => s.ServiceType == typeof(LocalizationService));

    Assert.Null(concreteErrorHandling);
    Assert.Null(concreteLocalization);
  }

  [Fact]
  public void AddWebApi_ShouldSupportChaining()
  {
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();

    // Act & Assert - Should support method chaining
    var result = services
        .AddWebApi(configuration)
        .AddWebApi(configuration);

    Assert.NotNull(result);
    Assert.Same(services, result);
  }

  private static void AddRequiredDependencies(IServiceCollection services)
  {
    // Add required dependencies for the services being tested
    services.AddHttpContextAccessor();
    services.AddLogging();
  }
}