using AppTemplate.Core.Application.Abstractions.Localization.Services;
using AppTemplate.Core.Infrastructure.Localization.Services;
using AppTemplate.Core.WebApi;

namespace AppTemplate.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();

        return services;
    }
}
