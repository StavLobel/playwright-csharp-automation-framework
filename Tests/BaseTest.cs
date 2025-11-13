using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Serilog;
using PlaywrightAutomation.Models;
using PlaywrightAutomation.Utils;

namespace PlaywrightAutomation.Tests;

public abstract class BaseTest : PageTest
{
    protected TestConfig Config { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    
    private static readonly Dictionary<string, (bool passed, double duration)> TestResults = new();
    private static readonly object TestResultsLock = new();
    private DateTime _testStartTime;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        try
        {
            // Configure Playwright reporter settings via environment variables
            ConfigurePlaywrightReporter();

            // Load configuration
            Config = ConfigManager.GetConfig();

            // Initialize Serilog logger
            Logger = LoggerFactory.CreateLogger(Config);

            Logger.Information("Test suite initialization started");
            Logger.Information("Base URL: {BaseUrl}", Config.BaseUrl);
            Logger.Information("Browser: {BrowserType}, Headless: {Headless}", 
                Config.Browser.BrowserType, Config.Browser.Headless);

            // Initialize test suite display
            TestLogger.InitializeSuite();

            Logger.Information("Test suite initialization completed");
        }
        catch (Exception ex)
        {
            TestLogger.Error($"Failed to initialize test suite: {ex.Message}");
            Logger?.Error(ex, "Test suite initialization failed");
            throw;
        }
    }

    /// <summary>
    /// Configure Playwright reporter settings
    /// </summary>
    private void ConfigurePlaywrightReporter()
    {
        // Set Playwright reporter output directory
        Environment.SetEnvironmentVariable("PLAYWRIGHT_JUNIT_OUTPUT_NAME", "Reports/junit-results.xml");
        Environment.SetEnvironmentVariable("PLAYWRIGHT_JSON_OUTPUT_NAME", "Reports/test-results.json");
        Environment.SetEnvironmentVariable("PLAYWRIGHT_HTML_OUTPUT_FOLDER", "Reports/playwright-report");
        
        // Configure screenshot and video capture
        Environment.SetEnvironmentVariable("PLAYWRIGHT_SCREENSHOT", "only-on-failure");
        Environment.SetEnvironmentVariable("PLAYWRIGHT_VIDEO", "retain-on-failure");
        Environment.SetEnvironmentVariable("PLAYWRIGHT_TRACE", "retain-on-failure");
        
        // Set artifacts output directory
        Environment.SetEnvironmentVariable("PLAYWRIGHT_ARTIFACTS_DIR", "Reports/test-artifacts");
    }

    [SetUp]
    public void Setup()
    {
        try
        {
            _testStartTime = DateTime.Now;
            var testName = TestContext.CurrentContext.Test.Name;
            TestLogger.TestStart(testName);
            Logger.Information("Starting test: {TestName}", testName);

            // PageTest handles Playwright initialization automatically
            // Page, Browser, Context are already available
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Test setup failed");
            throw;
        }
    }

    [TearDown]
    public void Teardown()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var outcome = TestContext.CurrentContext.Result.Outcome.Status;
        var passed = outcome == NUnit.Framework.Interfaces.TestStatus.Passed;
        var duration = (DateTime.Now - _testStartTime).TotalSeconds;

        // Record test result for summary
        lock (TestResultsLock)
        {
            TestResults[testName] = (passed, duration);
        }

        TestLogger.TestEnd(testName, passed);

        if (passed)
        {
            Logger.Information("Test passed: {TestName} in {Duration:F2}s", testName, duration);
        }
        else
        {
            Logger.Error("Test failed: {TestName} after {Duration:F2}s", testName, duration);
            
            var message = TestContext.CurrentContext.Result.Message;
            if (!string.IsNullOrEmpty(message))
            {
                Logger.Error("Failure message: {Message}", message);
            }
        }

        // PageTest handles cleanup automatically (screenshots, videos, context closure)
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        try
        {
            // Display detailed breakdown if there are test results
            lock (TestResultsLock)
            {
                if (TestResults.Count > 0)
                {
                    TestLogger.DisplayDetailedBreakdown(TestResults);
                }
            }

            // Display summary
            TestLogger.DisplaySummary();
            
            var (total, passed, failed, duration) = TestLogger.GetStatistics();
            Logger.Information("Test suite execution completed: {Total} total, {Passed} passed, {Failed} failed, {Duration:F2}s", 
                total, passed, failed, duration);
            
            LoggerFactory.CloseAndFlush();
        }
        catch (Exception ex)
        {
            TestLogger.Error($"Error during test suite teardown: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Helper method to get text content with retry logic
    /// </summary>
    protected async Task<string> GetTextWithRetry(string selector, int maxRetries = 3)
    {
        Exception? lastException = null;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await Page.WaitForSelectorAsync(selector, new() { Timeout = Config.Browser.Timeout });
                var element = await Page.QuerySelectorAsync(selector);
                
                if (element != null)
                {
                    var text = await element.TextContentAsync();
                    return text ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Logger.Warning("Retry {Attempt}/{MaxRetries} for selector: {Selector}", 
                    i + 1, maxRetries, selector);
                
                if (i < maxRetries - 1)
                {
                    await Task.Delay(1000); // Wait 1 second before retry
                }
            }
        }

        Logger.Error(lastException, "Failed to get text after {MaxRetries} retries for selector: {Selector}", 
            maxRetries, selector);
        throw new InvalidOperationException(
            $"Failed to get text for selector '{selector}' after {maxRetries} retries", 
            lastException);
    }
}
