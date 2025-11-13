using NUnit.Framework;
using FluentAssertions;
using PlaywrightAutomation.Pages;
using PlaywrightAutomation.Pages.Components;
using PlaywrightAutomation.Utils;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace PlaywrightAutomation.Tests.UI;

[TestFixture]
[Category("UI")]
[Category("DarkMode")]
public class DarkModeTest : BaseTest
{
    private WikipediaPlaywrightPage? _wikipediaPage;
    private ThemePanel? _themePanel;

    [SetUp]
    public new void Setup()
    {
        // Initialize page object
        _wikipediaPage = new WikipediaPlaywrightPage(Page, Config.BaseUrl);
        
        // Initialize theme panel component
        _themePanel = new ThemePanel(Page);
    }

    [Test]
    [Description("Verify that dark mode can be activated and properly applied on Wikipedia Playwright page")]
    public async Task VerifyDarkModeActivation()
    {
        // Arrange
        TestLogger.Info("Starting dark mode validation test...");

        // Act - Navigate to Wikipedia Playwright page
        TestLogger.Info("Navigating to Wikipedia Playwright page...");
        await _wikipediaPage!.NavigateToPlaywrightPage();
        TestLogger.Success("Navigation completed");

        // Act - Open theme panel
        TestLogger.Info("Opening theme panel...");
        await _wikipediaPage.OpenThemePanel();
        TestLogger.Success("Theme panel opened");

        // Act - Open color settings
        TestLogger.Info("Opening Color (beta) settings...");
        await _themePanel!.OpenColorSettings();
        TestLogger.Success("Color settings opened");

        // Act - Select dark theme
        TestLogger.Info("Selecting dark theme...");
        await _themePanel.SelectDarkTheme();
        TestLogger.Success("Dark theme selected");

        // Act - Verify theme change using multiple methods
        TestLogger.Info("Verifying dark mode activation...");

        // Verification Method 1: Check data-theme attribute
        var currentTheme = await _themePanel.GetCurrentTheme();
        TestLogger.TestDetail($"Current theme attribute: '{currentTheme}'");

        // Verification Method 2: Use ThemePanel's comprehensive verification
        var isDarkModeActive = await _themePanel.IsDarkModeActive();
        TestLogger.TestDetail($"Dark mode active (comprehensive check): {isDarkModeActive}");

        // Verification Method 3: Use Playwright's built-in Expect for DOM attribute
        var htmlLocator = Page.Locator("html");
        
        // Capture evidence of dark mode state
        TestLogger.Info("Capturing evidence of dark mode state...");
        var htmlClass = await htmlLocator.GetAttributeAsync("class") ?? string.Empty;
        var bodyClass = await Page.Locator("body").GetAttributeAsync("class") ?? string.Empty;
        TestLogger.TestDetail($"HTML classes: {htmlClass}");
        TestLogger.TestDetail($"Body classes: {bodyClass}");

        // Assert - Verify dark mode is active using FluentAssertions
        try
        {
            // Assert using data-theme attribute
            currentTheme.Should().Contain("dark", 
                because: "the data-theme attribute should contain 'dark' when dark mode is activated");

            // Assert using comprehensive verification
            isDarkModeActive.Should().BeTrue(
                because: "dark mode should be active after selecting the dark theme option");

            // Additional verification using Playwright Expect
            await Expect(htmlLocator).ToHaveAttributeAsync("data-theme", new Regex("dark", RegexOptions.IgnoreCase));

            TestLogger.Success("Dark mode successfully activated and verified");
            Logger.Information("Dark mode validation completed successfully. Theme: {Theme}", currentTheme);
        }
        catch (Exception ex)
        {
            TestLogger.Error($"Dark mode verification failed: {ex.Message}");
            TestLogger.Error($"Expected: dark mode active, Actual: theme='{currentTheme}', isDarkModeActive={isDarkModeActive}");
            
            Logger.Error(ex, "Dark mode validation failed. Theme: {Theme}, IsDarkModeActive: {IsDarkModeActive}", 
                currentTheme, isDarkModeActive);
            
            throw;
        }
    }
}
