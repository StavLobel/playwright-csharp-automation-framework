# Playwright C# Automation Framework

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Playwright](https://img.shields.io/badge/Playwright-1.48.0-2EAD33?logo=playwright)](https://playwright.dev/)
[![NUnit](https://img.shields.io/badge/NUnit-4.2.2-22B14C?logo=nunit)](https://nunit.org/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub](https://img.shields.io/github/stars/StavLobel/playwright-csharp-automation-framework?style=social)](https://github.com/StavLobel/playwright-csharp-automation-framework)

A lightweight automation framework built with C# and Playwright that implements Clean Architecture principles. The framework supports both UI and API testing with Page Object Model (POM) design, utility functions, and comprehensive HTML reporting.

## Features

- **Clean Architecture**: Organized into Pages, Tests, Models, Utils, and Reports layers
- **Page Object Model**: Maintainable UI test structure with reusable page objects
- **Dual Testing**: Both UI (Playwright) and API (MediaWiki) test capabilities
- **Text Normalization**: Advanced text processing and comparison utilities
- **Rich Reporting**: HTML reports with screenshots and detailed console output
- **Resilient API Client**: Built-in retry policies and timeout handling
- **Structured Logging**: Serilog integration for comprehensive test logging

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8 SDK** or higher
  - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verify installation: `dotnet --version`
- **Make** (for build automation)
  - macOS: Pre-installed or via Xcode Command Line Tools
  - Windows: Install via Chocolatey (`choco install make`) or use WSL
  - Linux: `sudo apt-get install build-essential`
- **PowerShell** (for Playwright browser installation)
  - macOS/Linux: Install PowerShell Core from https://github.com/PowerShell/PowerShell

## Quick Start

### 1. Install .NET 8 SDK

**macOS:**
```bash
brew install dotnet@8
```

**Windows:**
Download and install from https://dotnet.microsoft.com/download/dotnet/8.0

**Linux:**
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

### 2. Clone and Setup

```bash
# Clone the repository
git clone <repository-url>
cd PlaywrightAutomation

# Install dependencies and Playwright browsers
make install

# Build the project
make build
```

### 3. Run Tests

```bash
# Run all tests
make test

# Run UI tests only
make test-ui

# Run API tests only
make test-api

# Open HTML report
make report
```

## Project Structure

```
PlaywrightAutomation/
├── Makefile                    # Build automation commands
├── README.md                   # This file
├── .gitignore                  # Git ignore rules
├── PlaywrightAutomation.sln    # Solution file
├── PlaywrightAutomation.csproj # Project file
├── appsettings.json            # Configuration
├── Pages/                      # Page Object Model classes
│   ├── BasePage.cs
│   ├── WikipediaPlaywrightPage.cs
│   └── Components/
│       └── ThemePanel.cs
├── Tests/                      # Test classes
│   ├── BaseTest.cs
│   ├── UI/                     # UI tests
│   │   ├── DebuggingFeaturesTest.cs
│   │   ├── HyperlinkValidationTest.cs
│   │   └── DarkModeTest.cs
│   └── API/                    # API tests
│       └── MediaWikiApiTest.cs
├── Models/                     # Data models
│   ├── MediaWikiResponse.cs
│   ├── ParseResult.cs
│   └── TestConfig.cs
├── Utils/                      # Utility classes
│   ├── TextNormalizer.cs
│   ├── ConfigManager.cs
│   ├── MediaWikiClient.cs
│   └── TestLogger.cs
└── Reports/                    # Generated test reports
```

## Configuration

The framework uses `appsettings.json` for configuration. Copy the sample file and customize as needed:

```bash
cp appsettings.sample.json appsettings.json
```

### Quick Configuration Reference

```json
{
  "baseUrl": "https://en.wikipedia.org",
  "browser": {
    "browserType": "chromium",     // chromium, firefox, or webkit
    "headless": false,              // true for headless mode
    "timeout": 30000,               // Element timeout in ms
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
  },
  "logging": {
    "logLevel": "Information",
    "logFilePath": "./Reports/test-execution.log"
  }
}
```

For detailed configuration options and examples, see [CONFIGURATION.md](CONFIGURATION.md).

## Makefile Commands

| Command | Description |
|---------|-------------|
| `make install` | Restore NuGet packages and install Playwright browsers |
| `make build` | Compile the project |
| `make test` | Run all tests and generate HTML report |
| `make test-ui` | Run UI tests only |
| `make test-api` | Run API tests only |
| `make clean` | Remove build artifacts and reports |
| `make report` | Open the latest HTML report in browser |

## Architecture

The framework follows Clean Architecture principles with clear separation of concerns:

### Layers

1. **Test Layer**: Contains UI and API test classes
2. **Abstraction Layer**: Page objects and API clients
3. **Utility Layer**: Text processing, configuration, logging

### Key Components

- **BaseTest**: Provides common test setup, configuration loading, and logging
- **BasePage**: Base class for all page objects with common interactions
- **MediaWikiClient**: API client with retry policies and timeout handling
- **TextNormalizer**: Text processing utilities for content comparison
- **TestLogger**: Formatted console output using Spectre.Console

## Test Scenarios

### 1. Debugging Features Comparison
Extracts the "Debugging features" section from Wikipedia via both UI and API, normalizes the text, and compares unique word counts.

### 2. Hyperlink Validation
Validates that all technology names in the "Microsoft development tools" section are clickable hyperlinks.

### 3. Dark Mode Validation
Verifies that the dark theme can be activated and properly applied through the Wikipedia theme panel.

## Dependencies

- **Microsoft.Playwright** (1.48.0): Browser automation
- **Microsoft.Playwright.NUnit** (1.48.0): NUnit integration
- **NUnit** (4.2.2): Test framework
- **FluentAssertions** (6.12.1): Fluent assertion library
- **RestSharp** (112.1.0): HTTP client for API testing
- **Polly** (8.5.0): Resilience and retry policies
- **Serilog** (4.1.0): Structured logging
- **Spectre.Console** (0.49.1): Rich console output
- **HtmlAgilityPack** (1.11.71): HTML parsing
- **DiffPlex** (1.7.2): Text comparison

## Troubleshooting

### .NET SDK Not Found
```bash
# Verify .NET installation
dotnet --version

# If not installed, download from:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

**Solution:**
- Ensure .NET 8 SDK is installed (not just the runtime)
- Add .NET to your PATH if not automatically configured
- Restart your terminal after installation

### Playwright Browsers Not Installed
```bash
# Install browsers manually
pwsh bin/Debug/net8.0/playwright.ps1 install

# Or use the Makefile
make install
```

**Common Issues:**
- PowerShell not installed: Install PowerShell Core from https://github.com/PowerShell/PowerShell
- Permission denied: Run with elevated privileges or check file permissions
- Network issues: Ensure you have internet connectivity for browser downloads

### Tests Failing

**General Debugging Steps:**
```bash
# Clean and rebuild
make clean
make build

# Check configuration in appsettings.json
# Verify network connectivity to Wikipedia
```

**Specific Issues:**

1. **Element Not Found Errors**
   - Wikipedia page structure may have changed
   - Increase timeout in `appsettings.json` (browser.timeout)
   - Run tests in non-headless mode to observe behavior

2. **API Test Failures**
   - Check MediaWiki API endpoint is accessible
   - Verify network connectivity
   - Review API rate limiting (wait between test runs if needed)

3. **Text Comparison Mismatches**
   - Wikipedia content may have been updated
   - Check the detailed diff output in test results
   - Verify text normalization is working correctly

4. **Theme/Dark Mode Test Failures**
   - Clear browser cache and cookies
   - Ensure JavaScript is enabled
   - Check if Wikipedia's theme feature is available in your region

### Build Errors

**NuGet Package Restore Issues:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild
make build
```

**Compilation Errors:**
- Ensure you're using C# 12 and .NET 8
- Check for missing using statements
- Verify all NuGet packages are restored

### Permission Issues on macOS/Linux
```bash
# Make scripts executable
chmod +x dotnet-install.sh

# Fix Playwright permissions
chmod +x bin/Debug/net8.0/playwright.ps1
```

### Report Generation Issues

**HTML Report Not Generated:**
- Check `Reports/` directory exists
- Verify `appsettings.json` reporting.outputPath is correct
- Ensure tests completed execution (not interrupted)

**Cannot Open Report:**
```bash
# Manually open report
open Reports/playwright-report/index.html  # macOS
xdg-open Reports/playwright-report/index.html  # Linux
start Reports/playwright-report/index.html  # Windows
```

### Performance Issues

**Tests Running Slowly:**
- Reduce timeout values in configuration
- Enable headless mode for faster execution
- Disable video capture in `appsettings.json`
- Run tests in parallel (configured in `.runsettings`)

**High Memory Usage:**
- Close unnecessary browser instances
- Reduce viewport size in configuration
- Disable screenshot capture for passing tests

### Common Configuration Mistakes

1. **Wrong Browser Type**: Ensure `browserType` is one of: `chromium`, `firefox`, `webkit`
2. **Invalid Timeout Values**: Timeouts should be in milliseconds (e.g., 30000 = 30 seconds)
3. **Incorrect Paths**: Use relative paths for `outputPath` (e.g., `./Reports`)
4. **Missing API Endpoint**: Verify `mediaWiki.apiEndpoint` is set correctly

### Getting Help

If you encounter issues not covered here:
1. Check test execution logs in `Reports/test-execution.log`
2. Review console output for detailed error messages
3. Run tests with verbose logging enabled
4. Check Playwright documentation: https://playwright.dev/dotnet/
5. Review NUnit documentation: https://docs.nunit.org/

## Contributing

1. Follow the existing code structure and naming conventions
2. Add tests for new features
3. Update documentation as needed
4. Ensure all tests pass before submitting changes

## License

This project is licensed under the MIT License.
