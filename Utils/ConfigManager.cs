using System.Text.Json;
using PlaywrightAutomation.Models;

namespace PlaywrightAutomation.Utils;

public static class ConfigManager
{
    private static TestConfig? _config;
    private static readonly object _lock = new();

    public static TestConfig GetConfig()
    {
        if (_config != null)
            return _config;

        lock (_lock)
        {
            if (_config != null)
                return _config;

            _config = LoadConfig();
            return _config;
        }
    }

    private static TestConfig LoadConfig()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    $"Configuration file not found at: {configPath}. " +
                    "Please ensure appsettings.json exists in the project root.");
            }

            var jsonContent = File.ReadAllText(configPath);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new InvalidOperationException(
                    "Configuration file is empty. Please provide valid configuration in appsettings.json.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var config = JsonSerializer.Deserialize<TestConfig>(jsonContent, options);

            if (config == null)
            {
                throw new InvalidOperationException(
                    "Failed to deserialize configuration. Please check appsettings.json format.");
            }

            ValidateConfig(config);

            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid JSON format in appsettings.json: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Error loading configuration: {ex.Message}", ex);
        }
    }

    private static void ValidateConfig(TestConfig config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.BaseUrl))
            errors.Add("BaseUrl is required");

        if (string.IsNullOrWhiteSpace(config.Browser.BrowserType))
            errors.Add("Browser.BrowserType is required");

        if (config.Browser.Timeout <= 0)
            errors.Add("Browser.Timeout must be greater than 0");

        if (config.Browser.Viewport.Width <= 0)
            errors.Add("Browser.Viewport.Width must be greater than 0");

        if (config.Browser.Viewport.Height <= 0)
            errors.Add("Browser.Viewport.Height must be greater than 0");

        if (string.IsNullOrWhiteSpace(config.Reporting.OutputPath))
            errors.Add("Reporting.OutputPath is required");

        if (string.IsNullOrWhiteSpace(config.MediaWiki.ApiEndpoint))
            errors.Add("MediaWiki.ApiEndpoint is required");

        if (config.MediaWiki.Timeout <= 0)
            errors.Add("MediaWiki.Timeout must be greater than 0");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"Configuration validation failed:\n- {string.Join("\n- ", errors)}");
        }
    }

    // Method to reset config (useful for testing)
    internal static void ResetConfig()
    {
        lock (_lock)
        {
            _config = null;
        }
    }
}
