using AppTemplate.Application.Behaviors;
using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Caching;
using AppTemplate.Application.Services.EmailSenders;
using AppTemplate.Application.Services.Roles;
using Microsoft.Extensions.DependencyInjection;

namespace AppTemplate.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    AddMediatRBehaviors(services);
    AddApplicationServices(services);
    AddCache(services);
    return services;
  }

  private static void AddMediatRBehaviors(this IServiceCollection services)
  {
    services.AddMediatR(configuration =>
    {
      configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

      configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
      configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
      configuration.AddOpenBehavior(typeof(QueryCachingBehavior<,>));
    });
  }

  private static void AddApplicationServices(this IServiceCollection services)
  {
    services.AddScoped<IRolesService, RolesService>();
    services.AddScoped<IAppUsersService, AppUsersService>();

    // Add the account email service
    services.AddScoped<IAccountEmailService, AccountEmailService>();
  }

  private static void AddCache(IServiceCollection services)
  {
    services.AddScoped<ICacheService, CacheService>();
  }
}
