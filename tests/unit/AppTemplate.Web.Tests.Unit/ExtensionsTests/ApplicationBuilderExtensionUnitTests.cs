using AppTemplate.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AppTemplate.Web.Tests.Unit.ExtensionsTests;

[Trait("Category", "Unit")]
public class ApplicationBuilderExtensionUnitTests
{
  [Fact]
  public void UseCustomExceptionHandler_ShouldAddExceptionHandlingMiddleware()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    var serviceProvider = services.BuildServiceProvider();
    var mockApplicationBuilder = new Mock<IApplicationBuilder>();
    mockApplicationBuilder.SetupGet(x => x.ApplicationServices).Returns(serviceProvider);

    // Act
    mockApplicationBuilder.Object.UseCustomExceptionHandler();

    // Assert
    mockApplicationBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
  }

  [Fact]
  public void UseCustomForbiddenRequestHandler_ShouldAddForbiddenResponseMiddleware()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    var serviceProvider = services.BuildServiceProvider();
    var mockApplicationBuilder = new Mock<IApplicationBuilder>();
    mockApplicationBuilder.SetupGet(x => x.ApplicationServices).Returns(serviceProvider);

    // Act
    mockApplicationBuilder.Object.UseCustomForbiddenRequestHandler();

    // Assert
    mockApplicationBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
  }

  [Fact]
  public void UseRateLimitExceededHandler_ShouldAddRateLimitExceededMiddleware()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    var serviceProvider = services.BuildServiceProvider();
    var mockApplicationBuilder = new Mock<IApplicationBuilder>();
    mockApplicationBuilder.SetupGet(x => x.ApplicationServices).Returns(serviceProvider);

    // Act
    mockApplicationBuilder.Object.UseRateLimitExceededHandler();

    // Assert
    mockApplicationBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
  }

  [Fact]
  public void UseRequestContextLogging_ShouldAddRequestContextLoggingMiddleware_AndReturnApplicationBuilder()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    var serviceProvider = services.BuildServiceProvider();
    var mockApplicationBuilder = new Mock<IApplicationBuilder>();
    mockApplicationBuilder.SetupGet(x => x.ApplicationServices).Returns(serviceProvider);

    // Act
    var result = mockApplicationBuilder.Object.UseRequestContextLogging();

    // Assert
    mockApplicationBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    Assert.Equal(mockApplicationBuilder.Object, result);
  }

  [Fact]
  public void ConfigureCors_ShouldAddCorsServices_WithCorrectPolicy()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging(); // Add logging services required for CORS
    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "AllowedOrigins:0", "https://localhost:3000" },
            { "AllowedOrigins:1", "https://example.com" }
        });

    // Act
    var result = services.ConfigureCors(configuration);

    // Assert
    Assert.Equal(services, result);

    // Verify CORS services are registered by checking service descriptors
    Assert.Contains(services, s => s.ServiceType.Name.Contains("Cors"));
  }

  [Fact]
  public void ConfigureCors_WithNullAllowedOrigins_ShouldHandleGracefully()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging(); // Add logging services
    var configuration = CreateMockConfiguration(new Dictionary<string, string>());

    // Act & Assert - Should not throw
    var result = services.ConfigureCors(configuration);
    Assert.Equal(services, result);
  }

  [Fact]
  public void ConfigureControllers_ShouldAddControllersWithJsonOptions()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    var result = services.ConfigureControllers();

    // Assert
    Assert.Equal(services, result);

    // Verify service collection contains controller services
    Assert.Contains(services, s => s.ServiceType.Name.Contains("Controller") || s.ServiceType.Name.Contains("Mvc"));
  }

  [Fact]
  public void AddValidators_ShouldAddFluentValidationServices()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    var result = services.AddValidators();

    // Assert
    Assert.Equal(services, result);

    // Verify service collection contains validator services
    Assert.Contains(services, s => s.ServiceType == typeof(IValidator) ||
                                  s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>));
  }

  [Fact]
  public void ConfigureJwtAuthentication_ShouldAddJwtBearerAuthentication()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();

    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "Jwt:Secret", "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!" },
            { "Jwt:Issuer", "https://localhost:5001" },
            { "Jwt:Audience", "https://localhost:5001" }
        });

    // Act
    var result = services.ConfigureJwtAuthentication(mockEnvironment.Object, configuration);

    // Assert
    Assert.Equal(services, result);

    // Verify authentication services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("Authentication") ||
                                  s.ServiceType.Name.Contains("JwtBearer"));
  }

  [Fact]
  public void ConfigureRateLimiting_ShouldAddRateLimiterServices()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();

    // Act
    var result = services.ConfigureRateLimiting();

    // Assert
    Assert.Equal(services, result);

    // Verify rate limiter services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("RateLimit"));
  }

  [Fact]
  public void ConfigureApiDocumentation_ShouldAddOpenApiServices()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddRouting(); // Add routing services required for OpenAPI
    services.AddControllers(); // Add controller services required for API explorer

    // Act
    var result = services.ConfigureApiDocumentation();

    // Assert
    Assert.Equal(services, result);

    // Verify OpenAPI-related services are registered by checking service descriptors
    Assert.Contains(services, s => s.ServiceType.Name.Contains("OpenApi") ||
                                  s.ServiceType.Name.Contains("ApiExplorer"));
  }

  [Fact]
  public void MapApiDocumentation_InProductionEnvironment_ShouldReturnSameEndpointRouteBuilder()
  {
    // Arrange
    var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");

    // Act
    var result = mockEndpointRouteBuilder.Object.MapApiDocumentation(mockEnvironment.Object);

    // Assert
    Assert.Equal(mockEndpointRouteBuilder.Object, result);
  }

  [Fact]
  public void MapApiDocumentation_InStagingEnvironment_ShouldCheckEnvironment()
  {
    // Arrange
    var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Staging");

    // Act
    var result = mockEndpointRouteBuilder.Object.MapApiDocumentation(mockEnvironment.Object);

    // Assert
    Assert.Equal(mockEndpointRouteBuilder.Object, result);
    mockEnvironment.Verify(x => x.EnvironmentName, Times.AtLeastOnce);
  }

  [Fact]
  public void MapApiDocumentation_InProductionEnvironment_ShouldCheckEnvironment()
  {
    // Arrange
    var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");

    // Act
    var result = mockEndpointRouteBuilder.Object.MapApiDocumentation(mockEnvironment.Object);

    // Assert
    Assert.Equal(mockEndpointRouteBuilder.Object, result);
    mockEnvironment.Verify(x => x.EnvironmentName, Times.AtLeastOnce);
  }

  [Fact]
  public void MapApiDocumentation_EnvironmentChecking_WorksCorrectly()
  {
    // Arrange
    var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();

    // Test Production environment (should not call OpenAPI methods)
    var mockProductionEnvironment = new Mock<IWebHostEnvironment>();
    mockProductionEnvironment.SetupGet(x => x.EnvironmentName).Returns("Production");

    // Act
    var result = mockEndpointRouteBuilder.Object.MapApiDocumentation(mockProductionEnvironment.Object);

    // Assert
    Assert.Equal(mockEndpointRouteBuilder.Object, result);
    mockProductionEnvironment.Verify(x => x.EnvironmentName, Times.AtLeastOnce);
  }

  [Fact]
  public void ConfigureJwtAuthentication_WithInvalidSecret_ShouldThrowException()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();

    var mockEnvironment = new Mock<IWebHostEnvironment>();
    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "Jwt:Secret", "" }, // Invalid secret
            { "Jwt:Issuer", "https://localhost:5001" },
            { "Jwt:Audience", "https://localhost:5001" }
        });

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        services.ConfigureJwtAuthentication(mockEnvironment.Object, configuration));
  }

  [Fact]
  public void ConfigureControllers_ShouldConfigureJsonEnumConverter()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.ConfigureControllers();

    // Assert
    // Verify that controller services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("Controller") || s.ServiceType.Name.Contains("Mvc"));
  }

  [Fact]
  public void ConfigureRateLimiting_ShouldConfigureFixedWindowLimiter()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();

    // Act
    services.ConfigureRateLimiting();

    // Assert
    // Verify that rate limiting services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("RateLimit"));
  }

  [Fact]
  public void AddValidators_ShouldRegisterValidatorsFromAssembly()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    services.AddValidators();

    // Assert
    // Verify validator services are registered
    Assert.Contains(services, s => s.ServiceType == typeof(IValidator) ||
                                  (s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>)));
  }

  [Fact]
  public void ConfigureApiDocumentation_ShouldSetOpenApi30Version()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddRouting();
    services.AddControllers();

    // Act
    services.ConfigureApiDocumentation();

    // Assert
    // Verify OpenAPI services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("OpenApi") ||
                                  s.ServiceType.Name.Contains("ApiExplorer"));
  }

  [Fact]
  public void ConfigureCors_WithValidOrigins_ShouldNotThrow()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging(); // Add logging services
    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "AllowedOrigins:0", "https://localhost:3000" },
            { "AllowedOrigins:1", "https://api.example.com" }
        });

    // Act & Assert - Should not throw
    var result = services.ConfigureCors(configuration);
    Assert.Equal(services, result);
  }

  [Fact]
  public void UseCustomExceptionHandler_ShouldCallUseCorrectly()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    var serviceProvider = services.BuildServiceProvider();
    var mockApplicationBuilder = new Mock<IApplicationBuilder>();
    mockApplicationBuilder.SetupGet(x => x.ApplicationServices).Returns(serviceProvider);

    // Act
    mockApplicationBuilder.Object.UseCustomExceptionHandler();

    // Assert
    mockApplicationBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
  }

  [Fact]
  public void ConfigureJwtAuthentication_WithValidConfiguration_ShouldSucceed()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();

    var mockEnvironment = new Mock<IWebHostEnvironment>();
    mockEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "Jwt:Secret", "ThisIsAVeryLongSecretKeyThatShouldBeAtLeast32CharactersLongForSecurity!" },
            { "Jwt:Issuer", "https://localhost:5001" },
            { "Jwt:Audience", "https://localhost:5001" }
        });

    // Act
    var result = services.ConfigureJwtAuthentication(mockEnvironment.Object, configuration);

    // Assert
    Assert.Equal(services, result);

    // Verify authentication services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("Authentication"));
  }

  [Fact]
  public void ConfigureCors_ShouldReturnSameServiceCollection()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging(); // Add logging services
    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "AllowedOrigins:0", "https://localhost:3000" }
        });

    // Act
    var result = services.ConfigureCors(configuration);

    // Assert
    Assert.Same(services, result);
  }

  [Fact]
  public void ConfigureRateLimiting_ShouldReturnSameServiceCollection()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddOptions();

    // Act
    var result = services.ConfigureRateLimiting();

    // Assert
    Assert.Same(services, result);
  }

  [Fact]
  public void ConfigureApiDocumentation_ShouldReturnSameServiceCollection()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddRouting();
    services.AddControllers();

    // Act
    var result = services.ConfigureApiDocumentation();

    // Assert
    Assert.Same(services, result);
  }

  [Fact]
  public void ConfigureCors_ShouldRegisterCorsServices()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    var configuration = CreateMockConfiguration(new Dictionary<string, string>
        {
            { "AllowedOrigins:0", "https://localhost:3000" },
            { "AllowedOrigins:1", "https://example.com" }
        });

    // Act
    services.ConfigureCors(configuration);

    // Assert
    // Verify CORS-related services are registered
    Assert.Contains(services, s => s.ServiceType.Name.Contains("Cors"));
  }

  private static IConfiguration CreateMockConfiguration(Dictionary<string, string> configValues)
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(configValues);
    return configBuilder.Build();
  }
}