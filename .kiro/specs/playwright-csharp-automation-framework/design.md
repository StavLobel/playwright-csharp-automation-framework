# Design Document

## Overview

The C# + Playwright Automation Framework is a lightweight, maintainable testing solution that implements Clean Architecture principles. The framework supports both UI and API testing with a focus on Wikipedia's Playwright page validation scenarios. It uses the Page Object Model (POM) pattern for UI tests, includes a dedicated MediaWiki API client, provides text normalization utilities, and generates comprehensive HTML reports with readable console output.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Test Layer                            │
│  ┌──────────────────┐           ┌──────────────────┐       │
│  │   UI Tests       │           │   API Tests      │       │
│  │  (Playwright)    │           │  (HttpClient)    │       │
│  └──────────────────┘           └──────────────────┘       │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Abstraction Layer                         │
│  ┌──────────────────┐           ┌──────────────────┐       │
│  │   Page Objects   │           │  API Clients     │       │
│  │     (POM)        │           │  (MediaWiki)     │       │
│  └──────────────────┘           └──────────────────┘       │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                     Utility Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Text Utils   │  │ Config Mgr   │  │  Reporters   │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

### Directory Structure

```
PlaywrightAutomation/
├── Makefile
├── README.md
├── .gitignore
├── PlaywrightAutomation.sln
├── PlaywrightAutomation.csproj
├── appsettings.json
├── Pages/
│   ├── BasePage.cs
│   ├── WikipediaPlaywrightPage.cs
│   └── Components/
│       └── ThemePanel.cs
├── Tests/
│   ├── BaseTest.cs
│   ├── UI/
│   │   ├── DebuggingFeaturesTest.cs
│   │   ├── HyperlinkValidationTest.cs
│   │   └── DarkModeTest.cs
│   └── API/
│       ├── MediaWikiApiTest.cs
│       └── DebuggingFeaturesApiTest.cs
├── Models/
│   ├── MediaWikiResponse.cs
│   ├── ParseResult.cs
│   └── TestConfig.cs
├── Utils/
│   ├── TextNormalizer.cs
│   ├── ConfigManager.cs
│   ├── MediaWikiClient.cs
│   └── TestLogger.cs
└── Reports/
    └── (generated HTML reports)
```

## Components and Interfaces

### 1. Base Components

#### BaseTest
```csharp
// Inherits from PageTest (Microsoft.Playwright.NUnit)
// which provides built-in Page, Browser, Context, and BrowserType
public abstract class BaseTest : PageTest
{
    protected TestConfig Config { get; private set; }
    protected ILogger Logger { get; private set; }
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // Load configuration
        // Initialize Serilog logger
    }
    
    [SetUp]
    public async Task Setup()
    {
        // Additional setup if needed
        // PageTest already handles Playwright initialization
    }
    
    [TearDown]
    public async Task Teardown()
    {
        // PageTest handles cleanup automatically
        // Screenshots captured automatically on failure
    }
    
    protected async Task<string> GetTextWithRetry(string selector, int maxRetries = 3);
}
```

**Responsibilities:**
- Inherit from PageTest for automatic Playwright setup/teardown
- Load configuration from appsettings.json
- Initialize structured logging with Serilog
- Provide retry helpers for flaky operations
- Automatic screenshot capture on failure (built-in to PageTest)

**Design Rationale:**
- PageTest provides Page, Browser, Context automatically
- Built-in screenshot/video capture on test failure
- Reduces boilerplate code significantly

#### BasePage
```csharp
public abstract class BasePage
{
    protected IPage Page { get; }
    protected string BaseUrl { get; }
    
    protected BasePage(IPage page, string baseUrl);
    
    protected async Task<IElementHandle> WaitForSelector(string selector);
    protected async Task<string> GetTextContent(string selector);
    protected async Task ClickElement(string selector);
    protected async Task<bool> IsElementVisible(string selector);
}
```

**Responsibilities:**
- Provide common page interaction methods
- Handle element waiting and error handling
- Encapsulate Playwright API calls
- Serve as base for all page objects

### 2. Page Objects

#### WikipediaPlaywrightPage
```csharp
public class WikipediaPlaywrightPage : BasePage
{
    // Selectors
    private const string DebuggingFeaturesHeading = "//span[@id='Debugging_features']";
    private const string MicrosoftToolsSection = "//span[@id='Microsoft_development_tools']";
    private const string ThemePanelButton = "[aria-label='Tools']";
    
    public WikipediaPlaywrightPage(IPage page, string baseUrl) : base(page, baseUrl);
    
    public async Task NavigateToPlaywrightPage();
    public async Task<string> ExtractDebuggingFeaturesText();
    public async Task<List<string>> GetMicrosoftToolsTechnologies();
    public async Task<Dictionary<string, bool>> ValidateTechnologyLinks();
    public async Task OpenThemePanel();
}
```

**Responsibilities:**
- Navigate to Wikipedia Playwright page
- Extract text from specific sections
- Locate and interact with page elements
- Provide high-level page operations

#### ThemePanel (Component)
```csharp
public class ThemePanel
{
    private readonly IPage _page;
    
    private const string ColorBetaOption = "text=Color (beta)";
    private const string DarkThemeOption = "[data-event-name='dark']";
    private const string ThemeIndicator = "html[data-theme]";
    
    public ThemePanel(IPage page);
    
    public async Task OpenColorSettings();
    public async Task SelectDarkTheme();
    public async Task<string> GetCurrentTheme();
    public async Task<bool> IsDarkModeActive();
}
```

**Responsibilities:**
- Encapsulate theme panel interactions
- Handle color settings navigation
- Verify theme changes through DOM inspection

### 3. API Client

#### MediaWikiClient
```csharp
public class MediaWikiClient
{
    private readonly RestClient _restClient; // Using RestSharp
    private readonly IAsyncPolicy<RestResponse> _retryPolicy; // Using Polly
    private readonly ILogger _logger;
    private const string ApiEndpoint = "https://en.wikipedia.org/w/api.php";
    
    public MediaWikiClient(ILogger logger)
    {
        // Initialize RestSharp client
        // Configure Polly retry policy (3 retries with exponential backoff)
    }
    
    public async Task<ParseResult> GetPageSection(string pageTitle, string sectionTitle);
    public async Task<string> ParseSectionContent(string pageTitle, int sectionIndex);
    private async Task<int> FindSectionIndex(string pageTitle, string sectionTitle);
}
```

**Responsibilities:**
- Communicate with MediaWiki Parse API using RestSharp
- Retrieve page sections programmatically
- Parse API responses into models with System.Text.Json
- Handle API errors and retries using Polly resilience policies
- Log API calls and responses with Serilog

**Design Rationale:**
- RestSharp simplifies HTTP requests vs raw HttpClient
- Polly provides robust retry/timeout policies
- Structured logging helps debug API issues

### 4. Models

#### MediaWikiResponse
```csharp
public class MediaWikiResponse
{
    public Parse Parse { get; set; }
}

public class Parse
{
    public string Title { get; set; }
    public int PageId { get; set; }
    public Dictionary<string, Section> Sections { get; set; }
    public string Text { get; set; }
}

public class Section
{
    public int Index { get; set; }
    public string Line { get; set; }
    public int Level { get; set; }
}
```

**Responsibilities:**
- Model MediaWiki API responses
- Provide strongly-typed data structures
- Support JSON deserialization

#### TestConfig
```csharp
public class TestConfig
{
    public string BaseUrl { get; set; }
    public BrowserConfig Browser { get; set; }
    public ReportConfig Reporting { get; set; }
}

public class BrowserConfig
{
    public string BrowserType { get; set; } // chromium, firefox, webkit
    public bool Headless { get; set; }
    public int Timeout { get; set; }
    public ViewportSize Viewport { get; set; }
}

public class ReportConfig
{
    public string OutputPath { get; set; }
    public bool CaptureScreenshots { get; set; }
    public bool CaptureVideo { get; set; }
}
```

**Responsibilities:**
- Define configuration structure
- Support appsettings.json deserialization
- Provide type-safe configuration access

### 5. Utilities

#### TextNormalizer
```csharp
public static class TextNormalizer
{
    // Uses HtmlAgilityPack for HTML cleaning
    // Uses System.Text.RegularExpressions for pattern matching
    // Uses System.Globalization for Unicode normalization
    // Uses DiffPlex for detailed text comparison
    
    public static string Normalize(string text);
    public static string RemoveHtmlTags(string html); // Uses HtmlDocument.DocumentNode.InnerText
    public static string RemovePunctuation(string text); // Uses Regex
    public static string RemoveExcessiveWhitespace(string text); // Uses Regex
    public static HashSet<string> GetUniqueWords(string text);
    public static int CountUniqueWords(string text);
    public static DiffResult CompareTexts(string text1, string text2); // Uses DiffPlex
}
```

**Responsibilities:**
- Normalize text for comparison using built-in .NET and HtmlAgilityPack
- Remove HTML tags via HtmlAgilityPack's DOM parsing
- Remove punctuation and whitespace using Regex patterns
- Extract unique words using HashSet
- Provide detailed text diff using DiffPlex for better error reporting
- Provide consistent text processing

**Implementation Notes:**
- HtmlAgilityPack handles malformed HTML gracefully
- Regex patterns: `[^\w\s]` for punctuation, `\s+` for whitespace
- Unicode normalization via `text.Normalize(NormalizationForm.FormD)`
- DiffPlex generates side-by-side or inline diffs for debugging mismatches

#### ConfigManager
```csharp
public static class ConfigManager
{
    private static TestConfig _config;
    
    public static TestConfig GetConfig();
    private static TestConfig LoadConfig();
}
```

**Responsibilities:**
- Load configuration from appsettings.json
- Provide singleton access to configuration
- Handle configuration errors

#### TestLogger
```csharp
public static class TestLogger
{
    public static void Info(string message);
    public static void Success(string message);
    public static void Error(string message);
    public static void Warning(string message);
    public static void TestStart(string testName);
    public static void TestEnd(string testName, bool passed);
}
```

**Responsibilities:**
- Provide formatted console output
- Use Spectre.Console for colored output
- Log test execution progress
- Display test results clearly

## Data Models

### Text Comparison Data Flow

```
UI Extraction                    API Extraction
     │                                │
     ▼                                ▼
Raw HTML Text                   Raw JSON Text
     │                                │
     ▼                                ▼
TextNormalizer.Normalize()      TextNormalizer.Normalize()
     │                                │
     ▼                                ▼
Normalized Text                 Normalized Text
     │                                │
     ▼                                ▼
GetUniqueWords()                GetUniqueWords()
     │                                │
     ▼                                ▼
HashSet<string>                 HashSet<string>
     │                                │
     └────────────┬───────────────────┘
                  ▼
            Assert.AreEqual(uiCount, apiCount)
```

### Configuration Data Flow

```
appsettings.json
     │
     ▼
ConfigManager.GetConfig()
     │
     ▼
TestConfig Object
     │
     ├──► BaseTest (browser settings)
     ├──► WikipediaPlaywrightPage (base URL)
     └──► Reporters (output paths)
```

## Error Handling

### Test Failures

1. **Assertion Failures**: Use NUnit assertions with descriptive messages
2. **Element Not Found**: Implement retry logic with configurable timeout
3. **API Errors**: Log response details and throw custom exceptions
4. **Screenshot Capture**: Automatically capture on test failure

### Exception Strategy

```csharp
public class TestExecutionException : Exception
{
    public string TestName { get; set; }
    public string ScreenshotPath { get; set; }
    
    public TestExecutionException(string message, string testName) 
        : base(message)
    {
        TestName = testName;
    }
}
```

### Retry Logic

Using Polly for resilience policies:

```csharp
// API retry policy with exponential backoff
var apiRetryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, 
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            _logger.Warning($"API call failed. Retry {retryCount} after {timeSpan.TotalSeconds}s");
        });

// Timeout policy for API calls
var timeoutPolicy = Policy.TimeoutAsync(10);

// Combined policy
var combinedPolicy = Policy.WrapAsync(apiRetryPolicy, timeoutPolicy);
```

**Retry Strategy:**
- API calls: 3 retries with exponential backoff (2s, 4s, 8s)
- API timeout: 10 seconds per request
- Element interactions: Use Playwright's built-in waiting (no custom retry needed)
- Page navigation: Playwright handles retries automatically

## Testing Strategy

### Test Organization

#### UI Tests

1. **DebuggingFeaturesTest.cs**
   - Test: `CompareDebuggingFeaturesTextBetweenUIAndAPI()`
   - Extracts text via UI
   - Calls API test helper for API extraction
   - Normalizes both texts
   - Compares unique word counts

2. **HyperlinkValidationTest.cs**
   - Test: `ValidateAllMicrosoftToolsAreLinked()`
   - Navigates to section
   - Extracts all technology items
   - Validates each has <a> tag
   - Reports any non-linked items

3. **DarkModeTest.cs**
   - Test: `VerifyDarkModeActivation()`
   - Opens theme panel
   - Selects dark theme
   - Verifies DOM/CSS changes
   - Validates theme attribute

#### API Tests

1. **DebuggingFeaturesApiTest.cs**
   - Test: `ExtractDebuggingFeaturesSectionViaAPI()`
   - Calls MediaWiki Parse API
   - Retrieves section content
   - Returns normalized text
   - Can be used as helper for UI test

### Test Data

- Wikipedia URL: `https://en.wikipedia.org/wiki/Playwright_(software)`
- Target sections: "Debugging features", "Microsoft development tools"
- MediaWiki API endpoint: `https://en.wikipedia.org/w/api.php`

### Assertions

Using FluentAssertions for readable, expressive assertions:

```csharp
// Unique word count comparison with detailed diff
uiUniqueCount.Should().Be(apiUniqueCount, 
    because: "UI and API should extract the same content");

// If counts differ, show detailed diff using DiffPlex
if (uiUniqueCount != apiUniqueCount)
{
    var diff = TextNormalizer.CompareTexts(uiText, apiText);
    TestContext.WriteLine(diff.ToString());
}

// Hyperlink validation with clear message
isLinked.Should().BeTrue(
    because: $"Technology '{techName}' must be a clickable hyperlink");

// Dark mode validation with multiple checks
currentTheme.Should().Be("dark", 
    because: "Dark mode should be activated");
    
// Alternative: Check CSS property
await Expect(Page.Locator("html")).ToHaveAttributeAsync("data-theme", "dark");
```

**Design Rationale:**
- FluentAssertions provides better error messages
- Playwright's built-in Expect() for web-specific assertions
- DiffPlex shows exactly what differs when text comparison fails
- TestContext.WriteLine() outputs to test results for debugging

## Reporting

### HTML Report Structure

Using Playwright's built-in HTML reporter or Allure:

```
Reports/
├── index.html
├── data/
│   ├── test-results.json
│   └── attachments/
│       ├── screenshot-1.png
│       └── screenshot-2.png
└── assets/
    ├── styles.css
    └── scripts.js
```

### Console Output (Spectre.Console)

```
╔══════════════════════════════════════════════════════════╗
║           Playwright Automation Test Suite              ║
╚══════════════════════════════════════════════════════════╝

[▶] Running: CompareDebuggingFeaturesTextBetweenUIAndAPI
    ├─ Extracting UI text... ✓
    ├─ Calling MediaWiki API... ✓
    ├─ Normalizing texts... ✓
    ├─ Comparing unique words... ✓
    └─ UI: 45 words, API: 45 words
[✓] PASSED (2.3s)

[▶] Running: ValidateAllMicrosoftToolsAreLinked
    ├─ Navigating to section... ✓
    ├─ Extracting technologies... ✓
    ├─ Validating hyperlinks... ✓
    └─ Validated 8 hyperlinks
[✓] PASSED (1.1s)

[▶] Running: VerifyDarkModeActivation
    ├─ Opening theme panel... ✓
    ├─ Selecting dark theme... ✓
    ├─ Verifying theme change... ✓
[✓] PASSED (1.8s)

╔══════════════════════════════════════════════════════════╗
║                    Test Summary                          ║
╠══════════════════════════════════════════════════════════╣
║  Total:  3                                               ║
║  Passed: 3 ✓                                             ║
║  Failed: 0                                               ║
║  Time:   5.2s                                            ║
╚══════════════════════════════════════════════════════════╝
```

## Build and Execution

### Makefile Targets

```makefile
install:
    - Restore NuGet packages
    - Install Playwright browsers
    - Verify dependencies

build:
    - Compile C# project
    - Run code analysis

test:
    - Run all tests (UI + API)
    - Generate HTML report

test-ui:
    - Run UI tests only

test-api:
    - Run API tests only

clean:
    - Remove bin/obj directories
    - Clear test reports

report:
    - Open HTML report in browser
```

### Configuration (appsettings.json)

```json
{
  "baseUrl": "https://en.wikipedia.org",
  "browser": {
    "browserType": "chromium",
    "headless": false,
    "timeout": 30000,
    "viewport": {
      "width": 1920,
      "height": 1080
    }
  },
  "reporting": {
    "outputPath": "./Reports",
    "captureScreenshots": true,
    "captureVideo": false
  },
  "mediaWiki": {
    "apiEndpoint": "https://en.wikipedia.org/w/api.php",
    "timeout": 10000
  }
}
```

## Dependencies

### NuGet Packages

**Core Testing:**
- **Microsoft.Playwright** (v1.40+): Browser automation with built-in waiting, screenshots, video recording
- **Microsoft.Playwright.NUnit** (v1.40+): NUnit integration with PageTest base class
- **NUnit** (v3.14+): Test framework with parallel execution support
- **NUnit3TestAdapter** (v4.5+): Test adapter for Visual Studio/Rider

**Assertions & Validation:**
- **FluentAssertions** (v6.12+): Fluent assertion library with better error messages
- **Shouldly** (v4.2+): Alternative assertion library with readable syntax

**Data & Serialization:**
- **System.Text.Json** (built-in .NET 6+): Modern JSON serialization (faster than Newtonsoft.Json)
- **HtmlAgilityPack** (v1.11+): HTML parsing and tag removal

**Reporting & Logging:**
- **Spectre.Console** (v0.48+): Rich formatted console output with tables, progress bars, colors
- **Serilog** (v3.1+): Structured logging with file/console sinks
- **Serilog.Sinks.File** (v5.0+): File output for logs

**HTTP & API Testing:**
- **RestSharp** (v110+): Simplified HTTP client for API testing (alternative to raw HttpClient)
- **Polly** (v8.0+): Resilience and retry policies for API calls

**Utilities:**
- **Bogus** (v34+): Fake data generation for test data (if needed for future tests)
- **DiffPlex** (v1.7+): Text diffing library for detailed comparison reports

### System Requirements

- .NET 6.0 or higher
- Make (for Makefile execution)
- Supported OS: Windows, macOS, Linux

## Security Considerations

- No hardcoded credentials or sensitive data
- Configuration values externalized to appsettings.json
- API rate limiting considerations for MediaWiki API
- Browser security context isolation per test

## Performance Considerations

- Parallel test execution support via NUnit
- Browser context reuse within test class
- Lazy loading of page objects
- Efficient selector strategies (prefer data attributes over XPath)
- API response caching for repeated calls

## Extensibility

### Adding New Tests

1. Create test class inheriting from `BaseTest`
2. Use existing page objects or create new ones
3. Follow naming convention: `{Feature}Test.cs`
4. Add appropriate test categories/attributes

### Adding New Page Objects

1. Inherit from `BasePage`
2. Define selectors as constants
3. Implement page-specific methods
4. Follow single responsibility principle

### Adding New API Clients

1. Create client class with HttpClient
2. Define models for request/response
3. Implement error handling
4. Add to dependency injection if needed
