using Microsoft.Playwright;

namespace PlaywrightAutomation.Pages.Components;

/// <summary>
/// Component for interacting with Wikipedia's theme panel
/// </summary>
public class ThemePanel
{
    private readonly IPage _page;

    // Selectors for theme panel elements
    private const string ColorBetaOption = "text=Color (beta)";
    private const string DarkThemeButton = "[data-event-name='dark']";
    private const string ThemeIndicator = "html[data-theme]";

    public ThemePanel(IPage page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <summary>
    /// Open the Color (beta) settings in the theme panel
    /// </summary>
    public async Task OpenColorSettings()
    {
        try
        {
            // Locate and click the Color (beta) option
            var colorOption = _page.Locator(ColorBetaOption);
            await colorOption.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            await colorOption.ClickAsync();

            // Wait for the color settings to expand
            await _page.WaitForTimeoutAsync(500);
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException("Failed to open Color (beta) settings in theme panel", ex);
        }
    }

    /// <summary>
    /// Select the dark theme option
    /// </summary>
    public async Task SelectDarkTheme()
    {
        try
        {
            // Locate and click the dark theme button
            var darkThemeButton = _page.Locator(DarkThemeButton);
            await darkThemeButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            await darkThemeButton.ClickAsync();

            // Wait for theme to be applied
            await _page.WaitForTimeoutAsync(1000);
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException("Failed to select dark theme", ex);
        }
    }

    /// <summary>
    /// Get the current theme from the data-theme attribute on the html element
    /// </summary>
    public async Task<string> GetCurrentTheme()
    {
        try
        {
            var htmlElement = _page.Locator("html");
            var themeAttribute = await htmlElement.GetAttributeAsync("data-theme");
            return themeAttribute ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve current theme attribute", ex);
        }
    }

    /// <summary>
    /// Verify if dark mode is currently active using multiple verification strategies
    /// </summary>
    public async Task<bool> IsDarkModeActive()
    {
        try
        {
            // Strategy 1: Check data-theme attribute on html element
            var theme = await GetCurrentTheme();
            var hasDataTheme = theme.Contains("dark", StringComparison.OrdinalIgnoreCase);

            // Strategy 2: Check for dark mode class on html or body element
            var htmlElement = _page.Locator("html");
            var htmlClass = await htmlElement.GetAttributeAsync("class") ?? string.Empty;
            var hasDarkClass = htmlClass.Contains("dark", StringComparison.OrdinalIgnoreCase);

            var bodyElement = _page.Locator("body");
            var bodyClass = await bodyElement.GetAttributeAsync("class") ?? string.Empty;
            var hasBodyDarkClass = bodyClass.Contains("dark", StringComparison.OrdinalIgnoreCase);

            // Strategy 3: Check CSS background color (dark themes typically have dark backgrounds)
            var backgroundColor = await bodyElement.EvaluateAsync<string>(
                @"element => {
                    const style = window.getComputedStyle(element);
                    return style.backgroundColor;
                }"
            );

            // Parse RGB values to determine if background is dark
            var isDarkBackground = IsDarkColor(backgroundColor);

            // Return true if any verification strategy confirms dark mode
            return hasDataTheme || hasDarkClass || hasBodyDarkClass || isDarkBackground;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to verify dark mode status", ex);
        }
    }

    /// <summary>
    /// Helper method to determine if a color is dark based on RGB values
    /// </summary>
    private bool IsDarkColor(string rgbColor)
    {
        try
        {
            // Parse RGB color string (e.g., "rgb(0, 0, 0)" or "rgba(0, 0, 0, 1)")
            if (string.IsNullOrWhiteSpace(rgbColor))
            {
                return false;
            }

            var rgb = rgbColor.Replace("rgb(", "").Replace("rgba(", "").Replace(")", "").Split(',');
            if (rgb.Length < 3)
            {
                return false;
            }

            // Extract RGB values
            if (!int.TryParse(rgb[0].Trim(), out int r) ||
                !int.TryParse(rgb[1].Trim(), out int g) ||
                !int.TryParse(rgb[2].Trim(), out int b))
            {
                return false;
            }

            // Calculate luminance using the relative luminance formula
            // A color is considered dark if luminance is below 128 (on a 0-255 scale)
            var luminance = (0.299 * r + 0.587 * g + 0.114 * b);
            return luminance < 128;
        }
        catch
        {
            return false;
        }
    }
}
