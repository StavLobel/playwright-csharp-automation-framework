using NUnit.Framework;
using FluentAssertions;
using PlaywrightAutomation.Pages;
using PlaywrightAutomation.Utils;

namespace PlaywrightAutomation.Tests.UI;

[TestFixture]
[Category("UI")]
[Category("DebuggingFeatures")]
public class DebuggingFeaturesTest : BaseTest
{
    private WikipediaPlaywrightPage? _wikipediaPage;
    private MediaWikiClient? _mediaWikiClient;

    [SetUp]
    public new void Setup()
    {
        // Initialize page object
        _wikipediaPage = new WikipediaPlaywrightPage(Page, Config.BaseUrl);
        
        // Initialize MediaWiki API client
        _mediaWikiClient = new MediaWikiClient(Logger);
    }

    [Test]
    [Description("Compare debugging features text extracted from UI and API to verify content consistency")]
    public async Task CompareDebuggingFeaturesTextBetweenUIAndAPI()
    {
        // Arrange
        const string pageTitle = "Playwright_(software)";
        const string sectionTitle = "Debugging features";

        // Act - Navigate to Wikipedia Playwright page
        TestLogger.Info("Navigating to Wikipedia Playwright page...");
        await _wikipediaPage!.NavigateToPlaywrightPage();
        TestLogger.Success("Navigation completed");

        // Act - Extract debugging features text via UI
        TestLogger.Info("Extracting debugging features text from UI...");
        var uiText = await _wikipediaPage.ExtractDebuggingFeaturesText();
        TestLogger.Success($"UI text extracted: {uiText.Length} characters");

        // Act - Call MediaWiki API to get same section
        TestLogger.Info("Retrieving debugging features text from MediaWiki API...");
        var apiText = await _mediaWikiClient!.GetPageSection(pageTitle, sectionTitle);
        TestLogger.Success($"API text retrieved: {apiText.Length} characters");

        // Act - Normalize both texts
        TestLogger.Info("Normalizing texts...");
        var normalizedUiText = TextNormalizer.Normalize(uiText);
        var normalizedApiText = TextNormalizer.Normalize(apiText);
        TestLogger.Success("Text normalization completed");

        // Act - Count unique words in both texts
        TestLogger.Info("Counting unique words...");
        var uiUniqueCount = TextNormalizer.CountUniqueWords(normalizedUiText);
        var apiUniqueCount = TextNormalizer.CountUniqueWords(normalizedApiText);
        TestLogger.TestDetail($"UI unique words: {uiUniqueCount}, API unique words: {apiUniqueCount}");

        // Assert - Verify unique word counts are equal
        try
        {
            uiUniqueCount.Should().Be(apiUniqueCount,
                because: "UI and API should extract the same content with identical unique word counts");
            
            TestLogger.Success($"Unique word counts match: {uiUniqueCount} words");
        }
        catch (Exception)
        {
            // If assertion fails, output detailed diff
            TestLogger.Error($"Unique word count mismatch: UI={uiUniqueCount}, API={apiUniqueCount}");
            
            var diffModel = TextNormalizer.CompareTexts(normalizedUiText, normalizedApiText);
            var diffOutput = TextNormalizer.FormatDiffResults(diffModel);
            
            TestLogger.Error("Detailed text comparison:");
            TestContext.WriteLine(diffOutput);
            
            Logger.Error("Text comparison failed. Diff output written to test results.");
            
            throw;
        }
    }
}
