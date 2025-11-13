using Microsoft.Playwright;

namespace PlaywrightAutomation.Pages;

/// <summary>
/// Page object for Wikipedia's Playwright page
/// </summary>
public class WikipediaPlaywrightPage : BasePage
{
    // Selectors for page elements
    private const string DebuggingFeaturesHeading = "//span[@id='Debugging_features']";
    private const string DebuggingFeaturesSection = "//span[@id='Debugging_features']/ancestor::h2/following-sibling::*[following-sibling::h2 or following-sibling::h3]";
    private const string MicrosoftToolsHeading = "//span[@id='Microsoft_development_tools']";
    private const string MicrosoftToolsSection = "//span[@id='Microsoft_development_tools']/ancestor::h3/following-sibling::ul[1]";
    private const string ThemePanelButton = "[aria-label='Tools']";

    public WikipediaPlaywrightPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    /// <summary>
    /// Navigate to the Wikipedia Playwright page
    /// </summary>
    public async Task NavigateToPlaywrightPage()
    {
        var url = $"{BaseUrl}/wiki/Playwright_(software)";
        await Page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    /// <summary>
    /// Extract the complete text content from the "Debugging features" section
    /// </summary>
    public async Task<string> ExtractDebuggingFeaturesText()
    {
        // Wait for the heading to be visible
        await WaitForSelector(DebuggingFeaturesHeading);

        // Find the section content between the Debugging features heading and the next heading
        var sectionLocator = Page.Locator("//span[@id='Debugging_features']/ancestor::h2/following-sibling::*[self::p or self::ul or self::ol]");
        
        // Get all matching elements
        var count = await sectionLocator.CountAsync();
        var textParts = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var element = sectionLocator.Nth(i);
            
            // Check if we've reached the next section heading
            var tagName = await element.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            if (tagName == "h2" || tagName == "h3")
            {
                break;
            }

            var text = await element.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                textParts.Add(text);
            }
        }

        return string.Join(" ", textParts);
    }

    /// <summary>
    /// Get list of technology names from the Microsoft development tools subsection
    /// </summary>
    public async Task<List<string>> GetMicrosoftToolsTechnologies()
    {
        // Wait for the Microsoft tools section to be visible
        await WaitForSelector(MicrosoftToolsHeading);

        // Get the list items in the Microsoft tools section
        var listItemsLocator = Page.Locator("//span[@id='Microsoft_development_tools']/ancestor::h3/following-sibling::ul[1]/li");
        
        var count = await listItemsLocator.CountAsync();
        var technologies = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var item = listItemsLocator.Nth(i);
            var text = await item.TextContentAsync();
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                // Extract just the technology name (first part before any description)
                var techName = text.Trim().Split('\n')[0].Trim();
                technologies.Add(techName);
            }
        }

        return technologies;
    }

    /// <summary>
    /// Validate that each technology in the Microsoft tools section is a hyperlink
    /// Returns a dictionary mapping technology name to whether it's linked
    /// </summary>
    public async Task<Dictionary<string, bool>> ValidateTechnologyLinks()
    {
        // Wait for the Microsoft tools section to be visible
        await WaitForSelector(MicrosoftToolsHeading);

        // Get the list items in the Microsoft tools section
        var listItemsLocator = Page.Locator("//span[@id='Microsoft_development_tools']/ancestor::h3/following-sibling::ul[1]/li");
        
        var count = await listItemsLocator.CountAsync();
        var results = new Dictionary<string, bool>();

        for (int i = 0; i < count; i++)
        {
            var item = listItemsLocator.Nth(i);
            
            // Get the text content
            var text = await item.TextContentAsync();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var techName = text.Trim().Split('\n')[0].Trim();

            // Check if the item contains an anchor tag
            var anchorLocator = item.Locator("a");
            var anchorCount = await anchorLocator.CountAsync();
            var isLinked = anchorCount > 0;

            results[techName] = isLinked;
        }

        return results;
    }

    /// <summary>
    /// Open the theme panel on the right side of the page
    /// </summary>
    public async Task OpenThemePanel()
    {
        // Click the Tools button to open the panel
        await ClickElement(ThemePanelButton);
        
        // Wait for the panel to be visible
        await Page.WaitForTimeoutAsync(500); // Small delay for panel animation
    }
}
