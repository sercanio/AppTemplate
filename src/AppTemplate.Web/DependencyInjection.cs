using AppTemplate.Application.Services.ErrorHandling;
using AppTemplate.Application.Services.Localization;

namespace AppTemplate.Web;

public static class DependencyInjection
{
  public static IServiceCollection AddWebApi(this IServiceCollection services)
  {
    services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
    services.AddSingleton<ILocalizationService, LocalizationService>();

    return services;
  }
}
