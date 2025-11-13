using Microsoft.Playwright;

namespace PlaywrightAutomation.Pages;

/// <summary>
/// Base class for all page objects providing common interaction methods
/// </summary>
public abstract class BasePage
{
    protected IPage Page { get; }
    protected string BaseUrl { get; }

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page ?? throw new ArgumentNullException(nameof(page));
        BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    /// <summary>
    /// Wait for an element to be visible with timeout handling
    /// </summary>
    protected async Task<ILocator> WaitForSelector(string selector, int timeoutMs = 30000)
    {
        try
        {
            var locator = Page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return locator;
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException($"Element with selector '{selector}' was not found within {timeoutMs}ms", ex);
        }
    }

    /// <summary>
    /// Get text content from an element
    /// </summary>
    protected async Task<string> GetTextContent(string selector)
    {
        var locator = await WaitForSelector(selector);
        var text = await locator.TextContentAsync();
        return text ?? string.Empty;
    }

    /// <summary>
    /// Click an element
    /// </summary>
    protected async Task ClickElement(string selector)
    {
        var locator = await WaitForSelector(selector);
        await locator.ClickAsync();
    }

    /// <summary>
    /// Check if an element is visible
    /// </summary>
    protected async Task<bool> IsElementVisible(string selector, int timeoutMs = 5000)
    {
        try
        {
            var locator = Page.Locator(selector);
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return await locator.IsVisibleAsync();
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
