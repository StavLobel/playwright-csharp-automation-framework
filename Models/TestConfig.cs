using System.Text.Json.Serialization;

namespace PlaywrightAutomation.Models;

public class TestConfig
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("browser")]
    public BrowserConfig Browser { get; set; } = new();

    [JsonPropertyName("reporting")]
    public ReportConfig Reporting { get; set; } = new();

    [JsonPropertyName("mediaWiki")]
    public MediaWikiConfig MediaWiki { get; set; } = new();

    [JsonPropertyName("logging")]
    public LoggingConfig Logging { get; set; } = new();
}

public class BrowserConfig
{
    [JsonPropertyName("browserType")]
    public string BrowserType { get; set; } = "chromium";

    [JsonPropertyName("headless")]
    public bool Headless { get; set; } = false;

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 30000;

    [JsonPropertyName("viewport")]
    public ViewportSize Viewport { get; set; } = new();
}

public class ViewportSize
{
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;
}

public class ReportConfig
{
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = "./Reports";

    [JsonPropertyName("captureScreenshots")]
    public bool CaptureScreenshots { get; set; } = true;

    [JsonPropertyName("captureVideo")]
    public bool CaptureVideo { get; set; } = false;
}

public class MediaWikiConfig
{
    [JsonPropertyName("apiEndpoint")]
    public string ApiEndpoint { get; set; } = "https://en.wikipedia.org/w/api.php";

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 10000;
}

public class LoggingConfig
{
    [JsonPropertyName("logLevel")]
    public string LogLevel { get; set; } = "Information";

    [JsonPropertyName("logFilePath")]
    public string LogFilePath { get; set; } = "./Reports/test-execution.log";
}
