namespace AppTemplate.Application.Services.Localization;

public interface ILocalizationService
{
  string GetLocalizedString(string key, string language);
}
