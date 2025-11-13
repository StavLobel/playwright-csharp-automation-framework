using Serilog;
using Serilog.Events;
using PlaywrightAutomation.Models;

namespace PlaywrightAutomation.Utils;

public static class LoggerFactory
{
    private static ILogger? _logger;
    private static readonly object _lock = new();

    public static ILogger CreateLogger(TestConfig config)
    {
        if (_logger != null)
            return _logger;

        lock (_lock)
        {
            if (_logger != null)
                return _logger;

            var logLevel = ParseLogLevel(config.Logging.LogLevel);
            var logFilePath = config.Logging.LogFilePath;

            // Ensure the log directory exists
            var logDirectory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "PlaywrightAutomation")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.File(
                    logFilePath,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true)
                .CreateLogger();

            _logger.Information("Logger initialized with level: {LogLevel}", logLevel);
            _logger.Information("Log file path: {LogFilePath}", logFilePath);

            return _logger;
        }
    }

    public static ILogger GetLogger()
    {
        if (_logger == null)
        {
            throw new InvalidOperationException(
                "Logger has not been initialized. Call CreateLogger first.");
        }

        return _logger;
    }

    private static LogEventLevel ParseLogLevel(string logLevel)
    {
        return logLevel.ToLowerInvariant() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    public static void CloseAndFlush()
    {
        lock (_lock)
        {
            if (_logger != null)
            {
                Log.CloseAndFlush();
                _logger = null;
            }
        }
    }
}
