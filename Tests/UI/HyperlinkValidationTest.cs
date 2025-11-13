using NUnit.Framework;
using FluentAssertions;
using PlaywrightAutomation.Pages;
using PlaywrightAutomation.Utils;

namespace PlaywrightAutomation.Tests.UI;

[TestFixture]
[Category("UI")]
[Category("HyperlinkValidation")]
public class HyperlinkValidationTest : BaseTest
{
    private WikipediaPlaywrightPage? _wikipediaPage;

    [SetUp]
    public new void Setup()
    {
        // Initialize page object
        _wikipediaPage = new WikipediaPlaywrightPage(Page, Config.BaseUrl);
    }

    [Test]
    [Description("Validate that all technology names in the Microsoft development tools subsection are clickable hyperlinks")]
    public async Task ValidateAllMicrosoftToolsAreLinked()
    {
        // Arrange
        TestLogger.Info("Starting hyperlink validation test...");

        // Act - Navigate to Wikipedia Playwright page
        TestLogger.Info("Navigating to Wikipedia Playwright page...");
        await _wikipediaPage!.NavigateToPlaywrightPage();
        TestLogger.Success("Navigation completed");

        // Act - Get Microsoft tools technologies and validate links
        TestLogger.Info("Extracting Microsoft tools technologies and validating hyperlinks...");
        var technologyLinks = await _wikipediaPage.ValidateTechnologyLinks();
        TestLogger.Success($"Extracted {technologyLinks.Count} technology items");

        // Log each technology and its link status
        foreach (var tech in technologyLinks)
        {
            if (tech.Value)
            {
                TestLogger.TestDetail($"✓ {tech.Key} - is linked");
            }
            else
            {
                TestLogger.Warning($"✗ {tech.Key} - NOT linked");
            }
        }

        // Assert - Check if any items are not linked
        var unlinkedItems = technologyLinks.Where(t => !t.Value).ToList();

        if (unlinkedItems.Any())
        {
            var unlinkedNames = string.Join(", ", unlinkedItems.Select(t => t.Key));
            TestLogger.Error($"Found {unlinkedItems.Count} non-linked items: {unlinkedNames}");
            
            Logger.Error("Hyperlink validation failed. Non-linked items: {UnlinkedItems}", unlinkedNames);
            
            // Fail with detailed message
            Assert.Fail($"Expected all technology items to be hyperlinks, but found {unlinkedItems.Count} non-linked items: {unlinkedNames}");
        }

        // Assert - All items should be linked
        technologyLinks.Values.Should().AllSatisfy(isLinked => 
            isLinked.Should().BeTrue(because: "all Microsoft tools technologies must be clickable hyperlinks"));

        // Report success
        TestLogger.Success($"All {technologyLinks.Count} technology items are properly linked");
        Logger.Information("Hyperlink validation completed successfully. Validated {Count} hyperlinks", technologyLinks.Count);
    }
}
