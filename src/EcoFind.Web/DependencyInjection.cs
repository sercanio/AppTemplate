using Myrtus.Clarity.Core.Application.Abstractions.Localization.Services;
using Myrtus.Clarity.Core.Infrastructure.Localization.Services;
using Myrtus.Clarity.Core.WebAPI;

namespace EcoFind.Web;

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
