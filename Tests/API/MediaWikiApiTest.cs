using NUnit.Framework;
using FluentAssertions;
using PlaywrightAutomation.Utils;
using PlaywrightAutomation.Models;
using Serilog;

namespace PlaywrightAutomation.Tests.API;

[TestFixture]
[Category("API")]
[Category("MediaWiki")]
public class MediaWikiApiTest
{
    protected TestConfig Config { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    private MediaWikiClient? _mediaWikiClient;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        try
        {
            // Load configuration
            Config = ConfigManager.GetConfig();

            // Initialize Serilog logger
            Logger = LoggerFactory.CreateLogger(Config);

            Logger.Information("API test suite initialization started");
            Logger.Information("Base URL: {BaseUrl}", Config.BaseUrl);

            // Initialize test suite display
            TestLogger.InitializeSuite();

            Logger.Information("API test suite initialization completed");
        }
        catch (Exception ex)
        {
            TestLogger.Error($"Failed to initialize API test suite: {ex.Message}");
            Logger?.Error(ex, "API test suite initialization failed");
            throw;
        }
    }

    [SetUp]
    public void Setup()
    {
        try
        {
            var testName = TestContext.CurrentContext.Test.Name;
            TestLogger.TestStart(testName);
            Logger.Information("Starting API test: {TestName}", testName);

            // Initialize MediaWiki API client
            _mediaWikiClient = new MediaWikiClient(Logger);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "API test setup failed");
            throw;
        }
    }

    [TearDown]
    public void Teardown()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var outcome = TestContext.CurrentContext.Result.Outcome.Status;
        var passed = outcome == NUnit.Framework.Interfaces.TestStatus.Passed;

        TestLogger.TestEnd(testName, passed);

        if (passed)
        {
            Logger.Information("API test passed: {TestName}", testName);
        }
        else
        {
            Logger.Error("API test failed: {TestName}", testName);
            
            var message = TestContext.CurrentContext.Result.Message;
            if (!string.IsNullOrEmpty(message))
            {
                Logger.Error("Failure message: {Message}", message);
            }
        }
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        try
        {
            TestLogger.DisplaySummary();
            Logger.Information("API test suite execution completed");
            LoggerFactory.CloseAndFlush();
        }
        catch (Exception ex)
        {
            TestLogger.Error($"Error during API test suite teardown: {ex.Message}");
            throw;
        }
    }

    [Test]
    [Description("Extract debugging features section via MediaWiki API and verify content is retrieved")]
    public async Task ExtractDebuggingFeaturesSectionViaAPI()
    {
        // Arrange
        const string pageTitle = "Playwright_(software)";
        const string sectionTitle = "Debugging features";

        // Act - Call MediaWiki API to get section
        TestLogger.Info($"Retrieving section '{sectionTitle}' from page '{pageTitle}' via MediaWiki API...");
        var apiText = await _mediaWikiClient!.GetPageSection(pageTitle, sectionTitle);
        TestLogger.Success($"API text retrieved: {apiText.Length} characters");

        // Log API response details
        Logger.Information("API Response - Page: {PageTitle}, Section: {SectionTitle}, Content Length: {Length}",
            pageTitle, sectionTitle, apiText.Length);
        TestLogger.TestDetail($"Raw content length: {apiText.Length} characters");

        // Act - Normalize returned text
        TestLogger.Info("Normalizing retrieved text...");
        var normalizedText = TextNormalizer.Normalize(apiText);
        TestLogger.Success($"Text normalized: {normalizedText.Length} characters");

        // Log normalized text details
        var uniqueWordCount = TextNormalizer.CountUniqueWords(normalizedText);
        TestLogger.TestDetail($"Normalized length: {normalizedText.Length} characters");
        TestLogger.TestDetail($"Unique words: {uniqueWordCount}");
        
        Logger.Information("Normalized text - Length: {Length}, Unique Words: {UniqueWords}",
            normalizedText.Length, uniqueWordCount);

        // Assert - Section content is not empty
        normalizedText.Should().NotBeNullOrWhiteSpace(
            because: "the MediaWiki API should return content for the debugging features section");
        
        normalizedText.Length.Should().BeGreaterThan(0,
            because: "the normalized section content should contain text");

        TestLogger.Success($"Section content validated: {normalizedText.Length} characters, {uniqueWordCount} unique words");
    }
}
