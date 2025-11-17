using System.Reflection;
using System.Text.Json;
using IdentityPrvd.Options;

namespace IdentityPrvd.Services.Localization;

public class LocalizationService : ILocalizationService
{
    private const string DefaultLanguage = "en";
    private readonly Dictionary<string, Dictionary<string, string>> _resources = new();
    private readonly IdentityPrvdOptions _options;
    private readonly Assembly _assembly;

    public LocalizationService(IdentityPrvdOptions options)
    {
        _options = options;
        _assembly = Assembly.GetExecutingAssembly();
        LoadResources();
    }

    private void LoadResources()
    {
        var supportedLanguages = _options.Language.Languages ?? ["en", "uk"];
        
        foreach (var lang in supportedLanguages)
        {
            _resources[lang] = new Dictionary<string, string>();
            LoadResourceFile($"IdentityPrvd.Resources.Errors.{lang}.json", lang);
            LoadResourceFile($"IdentityPrvd.Resources.Validation.{lang}.json", lang);
        }
    }

    private void LoadResourceFile(string resourceName, string language)
    {
        try
        {
            // First try embedded resource (for NuGet package)
            // Try different possible resource name formats
            var possibleResourceNames = new[]
            {
                resourceName, // IdentityPrvd.Resources.Errors.en.json
                $"Resources.{Path.GetFileName(resourceName)}", // Resources.Errors.en.json
                resourceName.Replace("IdentityPrvd.Resources.", "Resources.") // Fallback format
            };

            Stream? stream = null;
            string? foundResourceName = null;
            
            foreach (var name in possibleResourceNames)
            {
                stream = _assembly.GetManifestResourceStream(name);
                if (stream != null)
                {
                    foundResourceName = name;
                    break;
                }
            }

            // Also try to find by partial name match (in case namespace differs)
            if (stream == null)
            {
                var allResources = _assembly.GetManifestResourceNames();
                var fileName = Path.GetFileName(resourceName);
                foundResourceName = allResources.FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                if (foundResourceName != null)
                {
                    stream = _assembly.GetManifestResourceStream(foundResourceName);
                }
            }

            if (stream != null)
            {
                using (stream)
                {
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    var resourceDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (resourceDict != null)
                    {
                        foreach (var kvp in resourceDict)
                        {
                            _resources[language][kvp.Key] = kvp.Value;
                        }
                    }
                }
                return;
            }

            // Fallback: Try file system (for development/debugging)
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", Path.GetFileName(resourceName)),
                Path.Combine(AppContext.BaseDirectory, "Resources", Path.GetFileName(resourceName)),
                Path.Combine(Directory.GetCurrentDirectory(), "Resources", Path.GetFileName(resourceName))
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var resourceDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (resourceDict != null)
                    {
                        foreach (var kvp in resourceDict)
                        {
                            _resources[language][kvp.Key] = kvp.Value;
                        }
                    }
                    break;
                }
            }
        }
        catch
        {
            // If resource file doesn't exist, continue without it
        }
    }

    public string GetString(string key, params object[] args)
    {
        return GetString(key, DefaultLanguage, args);
    }

    public string GetString(string key, string language, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(language) || !_resources.ContainsKey(language))
        {
            language = DefaultLanguage;
        }

        if (!_resources.ContainsKey(language))
        {
            language = DefaultLanguage;
        }

        var resourceDict = _resources[language];
        
        if (resourceDict.TryGetValue(key, out var value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }

        // Fallback to default language if key not found
        if (language != DefaultLanguage && _resources.ContainsKey(DefaultLanguage))
        {
            if (_resources[DefaultLanguage].TryGetValue(key, out var defaultValue))
            {
                return args.Length > 0 ? string.Format(defaultValue, args) : defaultValue;
            }
        }

        // Return key if not found
        return key;
    }
}
