using AppTemplate.Application.Services.AppUsers;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Core.Application.Abstractions.Behaviours;
using AppTemplate.Core.Application.Abstractions.Caching;
using AppTemplate.Core.Infrastructure.Caching;
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
    }

    private static void AddCache(IServiceCollection services)
    {
        services.AddScoped<ICacheService, CacheService>();
    }
}
