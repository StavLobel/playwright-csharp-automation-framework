# Implementation Plan

## Prerequisites

- [x] 0. Install .NET 8 SDK
  - Verify .NET is not already installed: `dotnet --version`
  - Install .NET 8 SDK for macOS: `brew install dotnet@8`
  - Alternative: Download from https://dotnet.microsoft.com/download/dotnet/8.0
  - Verify installation: `dotnet --version` should show 8.0.x or higher
  - _Requirements: 4.1, 7.2_

## Implementation Tasks

- [x] 1. Initialize project structure and configuration
  - Create .NET solution and project files with C# 12 and .NET 8
  - Add all required NuGet packages (Playwright, NUnit, RestSharp, Polly, Serilog, Spectre.Console, HtmlAgilityPack, FluentAssertions, DiffPlex)
  - Create directory structure: Pages/, Tests/UI/, Tests/API/, Models/, Utils/, Reports/
  - Create appsettings.json with browser, reporting, and MediaWiki API configuration
  - Create .gitignore for C# projects
  - Install Playwright browsers via `pwsh bin/Debug/net8.0/playwright.ps1 install`
  - _Requirements: 4.1, 4.2, 7.1, 7.3_

- [x] 2. Implement configuration and utilities foundation
  - [x] 2.1 Create TestConfig model classes
    - Implement TestConfig, BrowserConfig, ReportConfig, MediaWikiConfig models
    - Add System.Text.Json attributes for deserialization
    - _Requirements: 4.5, 5.2_
  
  - [x] 2.2 Create ConfigManager utility
    - Implement singleton pattern for configuration loading
    - Load and parse appsettings.json using System.Text.Json
    - Add error handling for missing or invalid configuration
    - _Requirements: 4.5, 5.2_
  
  - [x] 2.3 Create TestLogger utility with Spectre.Console
    - Implement colored console output methods (Info, Success, Error, Warning)
    - Add TestStart and TestEnd methods with formatted output
    - Create progress indicators and test summary formatting
    - _Requirements: 5.1, 5.3, 6.3, 6.4_
  
  - [x] 2.4 Configure Serilog for structured logging
    - Set up Serilog with console and file sinks
    - Configure log levels and output templates
    - Add logger initialization in test setup
    - _Requirements: 5.1, 6.4_

- [x] 3. Implement text normalization utilities
  - [x] 3.1 Create TextNormalizer class
    - Implement RemoveHtmlTags using HtmlAgilityPack
    - Implement RemovePunctuation using Regex pattern `[^\w\s]`
    - Implement RemoveExcessiveWhitespace using Regex pattern `\s+`
    - Implement Normalize method combining all cleaning steps with Unicode normalization
    - _Requirements: 1.3_
  
  - [x] 3.2 Add unique word extraction methods
    - Implement GetUniqueWords returning HashSet<string>
    - Implement CountUniqueWords for word count comparison
    - _Requirements: 1.4_
  
  - [x] 3.3 Add text comparison with DiffPlex
    - Implement CompareTexts method using DiffPlex
    - Return detailed diff results for debugging mismatches
    - _Requirements: 1.4, 5.1_

- [x] 4. Implement MediaWiki API client
  - [x] 4.1 Create MediaWikiResponse and ParseResult models
    - Define models for API response structure
    - Add JSON property attributes for deserialization
    - _Requirements: 1.2, 4.4_
  
  - [x] 4.2 Create MediaWikiClient with RestSharp
    - Initialize RestClient with API endpoint
    - Configure Polly retry policy (3 retries, exponential backoff)
    - Add timeout policy (10 seconds)
    - Inject Serilog logger for API call logging
    - _Requirements: 1.2, 4.4_
  
  - [x] 4.3 Implement section retrieval methods
    - Implement FindSectionIndex to locate section by title
    - Implement ParseSectionContent to get section text
    - Implement GetPageSection as main public method
    - Add error handling and logging for API failures
    - _Requirements: 1.2_

- [x] 5. Implement base page classes
  - [x] 5.1 Create BasePage class
    - Implement constructor accepting IPage and baseUrl
    - Add WaitForSelector with timeout handling
    - Add GetTextContent, ClickElement, IsElementVisible helper methods
    - _Requirements: 4.2_
  
  - [x] 5.2 Create BaseTest class inheriting from PageTest
    - Override OneTimeSetUp to load configuration and initialize logger
    - Override SetUp for additional test setup if needed
    - Override TearDown (PageTest handles cleanup automatically)
    - Add GetTextWithRetry helper method
    - _Requirements: 4.3, 5.4_

- [x] 6. Implement Wikipedia Playwright page objects
  - [x] 6.1 Create WikipediaPlaywrightPage class
    - Define selectors for Debugging features section, Microsoft tools section
    - Implement NavigateToPlaywrightPage method
    - Implement ExtractDebuggingFeaturesText to get section content
    - _Requirements: 1.1, 2.1, 4.2_
  
  - [x] 6.2 Add Microsoft tools section methods
    - Implement GetMicrosoftToolsTechnologies to extract technology names
    - Implement ValidateTechnologyLinks to check each item for <a> tag
    - Return dictionary mapping technology name to isLinked boolean
    - _Requirements: 2.2, 2.3_
  
  - [x] 6.3 Add theme panel interaction methods
    - Implement OpenThemePanel to access right-side panel
    - Add navigation to theme settings
    - _Requirements: 3.1_

- [x] 7. Create ThemePanel component
  - [x] 7.1 Implement ThemePanel class
    - Define selectors for Color (beta) option and dark theme button
    - Implement OpenColorSettings method
    - Implement SelectDarkTheme method
    - _Requirements: 3.1, 3.2_
  
  - [x] 7.2 Add theme verification methods
    - Implement GetCurrentTheme to read data-theme attribute
    - Implement IsDarkModeActive to verify theme change
    - Use multiple verification strategies (DOM, CSS, class attributes)
    - _Requirements: 3.3_

- [x] 8. Implement UI test: Debugging features comparison
  - [x] 8.1 Create DebuggingFeaturesTest class
    - Inherit from BaseTest
    - Set up test with proper test attributes and categories
    - _Requirements: 1.1, 1.2, 1.3, 1.4_
  
  - [x] 8.2 Implement CompareDebuggingFeaturesTextBetweenUIAndAPI test
    - Navigate to Wikipedia Playwright page using page object
    - Extract debugging features text via UI
    - Call MediaWikiClient to get same section via API
    - Normalize both texts using TextNormalizer
    - Count unique words in both texts
    - Assert counts are equal using FluentAssertions
    - If assertion fails, output detailed diff using DiffPlex
    - Log test progress with TestLogger
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

- [x] 9. Implement UI test: Hyperlink validation
  - [x] 9.1 Create HyperlinkValidationTest class
    - Inherit from BaseTest
    - Set up test with proper attributes
    - _Requirements: 2.1, 2.2, 2.3_
  
  - [x] 9.2 Implement ValidateAllMicrosoftToolsAreLinked test
    - Navigate to Wikipedia Playwright page
    - Use page object to get Microsoft tools technologies
    - Validate each technology is a hyperlink
    - Assert all items are linked using FluentAssertions
    - Report count of validated hyperlinks
    - Fail test with details if any item is not linked
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [x] 10. Implement UI test: Dark mode validation
  - [x] 10.1 Create DarkModeTest class
    - Inherit from BaseTest
    - Set up test with proper attributes
    - _Requirements: 3.1, 3.2, 3.3_
  
  - [x] 10.2 Implement VerifyDarkModeActivation test
    - Navigate to Wikipedia Playwright page
    - Use page object to open theme panel
    - Use ThemePanel component to select dark theme
    - Verify theme change using multiple methods (DOM attribute, CSS, Playwright Expect)
    - Assert dark mode is active using FluentAssertions
    - Capture evidence of dark mode state
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 11. Implement API test: MediaWiki section extraction
  - [x] 11.1 Create MediaWikiApiTest class
    - Inherit from BaseTest (for configuration access)
    - Set up test with proper attributes
    - _Requirements: 1.2_
  
  - [x] 11.2 Implement ExtractDebuggingFeaturesSectionViaAPI test
    - Initialize MediaWikiClient
    - Call GetPageSection for "Playwright_(software)" page and "Debugging features" section
    - Normalize returned text
    - Assert section content is not empty
    - Log API response details
    - _Requirements: 1.2, 4.4_

- [x] 12. Configure test execution and reporting
  - [x] 12.1 Configure NUnit test runner
    - Set up parallel execution settings in .runsettings file
    - Configure test output settings
    - _Requirements: 5.2_
  
  - [x] 12.2 Configure Playwright HTML reporter
    - Add playwright.config.json or configure via code
    - Set report output path to ./Reports
    - Enable screenshot capture on failure
    - Configure trace collection
    - _Requirements: 5.1, 5.4_
  
  - [x] 12.3 Add test result summary output
    - Implement test execution summary with Spectre.Console
    - Display total tests, passed, failed, execution time
    - Format output in a table or panel
    - _Requirements: 5.3, 6.5_

- [x] 13. Create Makefile for build automation
  - [x] 13.1 Create Makefile with install target
    - Add command to restore NuGet packages
    - Add command to install Playwright browsers
    - Add dependency verification
    - _Requirements: 6.1, 7.2_
  
  - [x] 13.2 Add build and test targets
    - Add build target to compile the project
    - Add test target to run all tests with HTML report generation
    - Add test-ui target for UI tests only
    - Add test-api target for API tests only
    - Add clean target to remove build artifacts
    - Add report target to open HTML report in browser
    - _Requirements: 6.1, 6.2_

- [x] 14. Create documentation and repository files
  - [x] 14.1 Create comprehensive README.md
    - Add project overview and features
    - Document prerequisites (.NET 8, Make)
    - Add setup instructions using Makefile
    - Document test execution commands
    - Explain project architecture and structure
    - Add configuration guide for appsettings.json
    - Include troubleshooting section
    - _Requirements: 7.2, 7.3_
  
  - [x] 14.2 Create .gitignore file
    - Add standard C# ignores (bin/, obj/, .vs/)
    - Add test output ignores (TestResults/, Reports/)
    - Add IDE-specific ignores
    - Add Playwright artifacts ignores
    - _Requirements: 7.1_
  
  - [x] 14.3 Add sample configuration files
    - Create appsettings.json with placeholder values
    - Add comments explaining each configuration option
    - _Requirements: 7.5_

- [x] 15. Final integration and validation
  - [x] 15.1 Run full test suite
    - Execute `make test` to run all tests
    - Verify all tests pass
    - Check HTML report generation
    - Validate console output formatting
    - _Requirements: 5.1, 5.3, 6.3, 6.4_
  
  - [x] 15.2 Verify repository structure
    - Confirm all directories and files are in place
    - Validate .gitignore is working correctly
    - Ensure no sensitive data or build artifacts are committed
    - _Requirements: 7.1, 7.3, 7.4_
  
  - [x] 15.3 Test clean installation workflow
    - Clone repository to fresh directory
    - Run `make install` to set up dependencies
    - Run `make build` to compile project
    - Run `make test` to execute tests
    - Verify README instructions are accurate
    - _Requirements: 6.1, 7.2_
