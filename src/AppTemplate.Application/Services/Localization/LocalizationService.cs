using System.Text;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AppTemplate.Application.Services.Localization;

public class LocalizationService : ILocalizationService
{
  private readonly Dictionary<string, Dictionary<string, string>> _localizedMessages;
  private readonly ILogger<LocalizationService> _logger;

  public LocalizationService(ILogger<LocalizationService> logger)
  {
    _logger = logger;
    _localizedMessages = LoadLocalizedMessages();
  }

  public string GetLocalizedString(string key, string language)
  {
    if (_localizedMessages.TryGetValue(language, out var messages) && messages.TryGetValue(key, out var message))
    {
      return message;
    }

    var baseLanguage = language.Split('-')[0];
    if (_localizedMessages.TryGetValue(baseLanguage, out var baseMessages) && baseMessages.TryGetValue(key, out var baseMessage))
    {
      return baseMessage;
    }

    if (_localizedMessages.TryGetValue("en-US", out var defaultMessages) && defaultMessages.TryGetValue(key, out var defaultMessage))
    {
      return defaultMessage;
    }

    return key;
  }

  private Dictionary<string, Dictionary<string, string>> LoadLocalizedMessages()
  {
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    var localizedMessages = new Dictionary<string, Dictionary<string, string>>();
    var resourceDirectory = Path.Combine(AppContext.BaseDirectory, "Resources");

    if (Directory.Exists(resourceDirectory))
    {
      var yamlFiles = Directory.GetFiles(resourceDirectory, "*.yaml");

      foreach (var filePath in yamlFiles)
      {
        try
        {
          var language = Path.GetFileNameWithoutExtension(filePath);

          // Use StreamReader with UTF8 encoding and pass it directly to the deserializer
          using (var reader = new StreamReader(filePath, Encoding.UTF8))
          {
            var messages = deserializer.Deserialize<Dictionary<object, object>>(reader);
            localizedMessages[language] = FlattenDictionary(messages);
          }

          _logger.LogInformation("Successfully loaded {Count} keys for language {Language}", localizedMessages[language].Count, language);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error deserializing YAML file: {FilePath}", filePath);
        }
      }
    }

    return localizedMessages;
  }

  private Dictionary<string, string> FlattenDictionary(IDictionary<object, object> nestedDict, string parentKey = "")
  {
    var flatDict = new Dictionary<string, string>();

    foreach (var pair in nestedDict)
    {
      // Ensure the key is handled as string
      var keyPart = pair.Key as string;
      if (keyPart == null)
      {
        _logger.LogWarning("Skipping key because it is not a string: {Key}", pair.Key);
        continue;
      }

      var key = string.IsNullOrEmpty(parentKey) ? keyPart : $"{parentKey}.{keyPart}";

      if (pair.Value is IDictionary<object, object> nested)
      {
        foreach (var innerPair in FlattenDictionary(nested, key))
        {
          flatDict[innerPair.Key] = innerPair.Value;
        }
      }
      else if (pair.Value is string stringValue)
      {
        flatDict[key] = stringValue;
      }
      else
      {
        // Handle scalar values
        flatDict[key] = pair.Value != null ? Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
      }
    }

    return flatDict;
  }
}
