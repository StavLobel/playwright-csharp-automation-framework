# Configuration Guide

This document explains all configuration options available in `appsettings.json`.

## Configuration File Structure

The `appsettings.json` file contains all runtime configuration for the Playwright C# Automation Framework. Copy `appsettings.sample.json` to `appsettings.json` and customize as needed.

```bash
cp appsettings.sample.json appsettings.json
```

## Configuration Options

### Base URL

```json
"baseUrl": "https://en.wikipedia.org"
```

**Description:** The base URL for the application under test.

**Options:**
- Any valid HTTP/HTTPS URL
- Used as the starting point for navigation in tests

**Example:**
```json
"baseUrl": "https://en.wikipedia.org"
```

---

### Browser Configuration

```json
"browser": {
  "browserType": "chromium",
  "headless": false,
  "timeout": 30000,
  "viewport": {
    "width": 1920,
    "height": 1080
  }
}
```

#### browserType

**Description:** The browser engine to use for test execution.

**Options:**
- `"chromium"` - Google Chrome/Microsoft Edge (recommended)
- `"firefox"` - Mozilla Firefox
- `"webkit"` - Safari (WebKit engine)

**Default:** `"chromium"`

**Example:**
```json
"browserType": "firefox"
```

#### headless

**Description:** Whether to run the browser in headless mode (without UI).

**Options:**
- `true` - Run without visible browser window (faster, suitable for CI/CD)
- `false` - Show browser window (useful for debugging)

**Default:** `false`

**Example:**
```json
"headless": true
```

**When to use:**
- `true`: CI/CD pipelines, automated test runs, performance testing
- `false`: Local development, debugging, visual verification

#### timeout

**Description:** Default timeout for element interactions in milliseconds.

**Options:**
- Any positive integer (milliseconds)
- Recommended range: 10000-60000 (10-60 seconds)

**Default:** `30000` (30 seconds)

**Example:**
```json
"timeout": 45000
```

**Notes:**
- Increase for slow-loading pages or slow network connections
- Decrease for faster feedback during development
- Playwright will wait up to this duration for elements to appear

#### viewport

**Description:** Browser window size for test execution.

**Options:**
- `width`: Integer (pixels), recommended: 1280-1920
- `height`: Integer (pixels), recommended: 720-1080

**Default:** `1920x1080`

**Common Presets:**
```json
// Desktop Full HD
"viewport": { "width": 1920, "height": 1080 }

// Desktop HD
"viewport": { "width": 1280, "height": 720 }

// Tablet (iPad)
"viewport": { "width": 768, "height": 1024 }

// Mobile (iPhone)
"viewport": { "width": 375, "height": 667 }
```

---

### Reporting Configuration

```json
"reporting": {
  "outputPath": "./Reports",
  "captureScreenshots": true,
  "captureVideo": false
}
```

#### outputPath

**Description:** Directory path where test reports and artifacts are saved.

**Options:**
- Relative path (e.g., `"./Reports"`)
- Absolute path (e.g., `"/Users/username/test-reports"`)

**Default:** `"./Reports"`

**Example:**
```json
"outputPath": "./TestOutput"
```

**Notes:**
- Directory will be created automatically if it doesn't exist
- Use relative paths for portability across environments

#### captureScreenshots

**Description:** Whether to capture screenshots during test execution.

**Options:**
- `true` - Capture screenshots on test failure (recommended)
- `false` - No screenshots captured

**Default:** `true`

**Example:**
```json
"captureScreenshots": true
```

**Notes:**
- Screenshots are automatically captured on test failures
- Useful for debugging and test result analysis
- Increases storage requirements

#### captureVideo

**Description:** Whether to record video of test execution.

**Options:**
- `true` - Record video for all tests
- `false` - No video recording (recommended for faster execution)

**Default:** `false`

**Example:**
```json
"captureVideo": true
```

**Notes:**
- Video recording significantly increases execution time and storage
- Useful for debugging complex interactions
- Only enable when needed for specific debugging scenarios

---

### MediaWiki API Configuration

```json
"mediaWiki": {
  "apiEndpoint": "https://en.wikipedia.org/w/api.php",
  "timeout": 10000
}
```

#### apiEndpoint

**Description:** The MediaWiki API endpoint URL for API tests.

**Options:**
- Any valid MediaWiki API endpoint
- Wikipedia: `https://en.wikipedia.org/w/api.php`
- Other wikis: `https://<wiki-domain>/w/api.php`

**Default:** `"https://en.wikipedia.org/w/api.php"`

**Example:**
```json
"apiEndpoint": "https://en.wikipedia.org/w/api.php"
```

#### timeout

**Description:** Timeout for API requests in milliseconds.

**Options:**
- Any positive integer (milliseconds)
- Recommended range: 5000-30000 (5-30 seconds)

**Default:** `10000` (10 seconds)

**Example:**
```json
"timeout": 15000
```

**Notes:**
- Increase for slow API responses or network latency
- API client includes retry logic (3 retries with exponential backoff)

---

### Logging Configuration

```json
"logging": {
  "logLevel": "Information",
  "logFilePath": "./Reports/test-execution.log"
}
```

#### logLevel

**Description:** Minimum log level for structured logging.

**Options:**
- `"Verbose"` - Most detailed, includes all logs
- `"Debug"` - Detailed debugging information
- `"Information"` - General informational messages (recommended)
- `"Warning"` - Warning messages only
- `"Error"` - Error messages only
- `"Fatal"` - Critical errors only

**Default:** `"Information"`

**Example:**
```json
"logLevel": "Debug"
```

**When to use:**
- `Verbose/Debug`: Troubleshooting specific issues
- `Information`: Normal test execution
- `Warning/Error`: Production environments or CI/CD

#### logFilePath

**Description:** File path where structured logs are written.

**Options:**
- Relative path (e.g., `"./Reports/test-execution.log"`)
- Absolute path (e.g., `"/var/log/playwright-tests.log"`)

**Default:** `"./Reports/test-execution.log"`

**Example:**
```json
"logFilePath": "./Logs/tests.log"
```

**Notes:**
- Log files are appended (not overwritten)
- Rotate or clean up log files periodically
- Logs include timestamps, test names, and detailed execution information

---

## Environment-Specific Configuration

You can create multiple configuration files for different environments:

```bash
appsettings.json              # Default configuration
appsettings.Development.json  # Development overrides
appsettings.Production.json   # Production overrides
appsettings.CI.json          # CI/CD overrides
```

**Example Development Configuration:**
```json
{
  "browser": {
    "browserType": "chromium",
    "headless": false,
    "timeout": 30000
  },
  "logging": {
    "logLevel": "Debug"
  }
}
```

**Example CI/CD Configuration:**
```json
{
  "browser": {
    "browserType": "chromium",
    "headless": true,
    "timeout": 60000
  },
  "reporting": {
    "captureScreenshots": true,
    "captureVideo": false
  },
  "logging": {
    "logLevel": "Information"
  }
}
```

---

## Configuration Best Practices

1. **Version Control:**
   - Commit `appsettings.sample.json` to version control
   - Add `appsettings.json` to `.gitignore` if it contains sensitive data
   - Use environment-specific files for different deployment targets

2. **Security:**
   - Never commit credentials or API keys to version control
   - Use environment variables for sensitive configuration
   - Keep production configurations separate and secure

3. **Performance:**
   - Use `headless: true` in CI/CD for faster execution
   - Disable video capture unless debugging specific issues
   - Adjust timeouts based on your network and application performance

4. **Debugging:**
   - Set `headless: false` to watch tests execute
   - Increase `logLevel` to `Debug` or `Verbose` for troubleshooting
   - Enable `captureScreenshots` to analyze failures

5. **Maintenance:**
   - Review and update timeouts periodically
   - Clean up old reports and logs regularly
   - Document any custom configuration changes

---

## Troubleshooting Configuration Issues

### Configuration Not Loading

**Symptoms:** Tests use default values instead of your configuration

**Solutions:**
1. Verify `appsettings.json` exists in the project root
2. Check JSON syntax is valid (use a JSON validator)
3. Ensure file is copied to output directory (check `.csproj` settings)

### Invalid Browser Type

**Symptoms:** Error: "Browser type 'xyz' is not supported"

**Solutions:**
1. Use only: `chromium`, `firefox`, or `webkit`
2. Check for typos in the configuration
3. Ensure Playwright browsers are installed: `make install`

### Timeout Errors

**Symptoms:** Tests fail with "Timeout exceeded" errors

**Solutions:**
1. Increase `browser.timeout` value
2. Increase `mediaWiki.timeout` for API tests
3. Check network connectivity
4. Verify target application is responsive

### Report Generation Failures

**Symptoms:** Reports not generated or empty

**Solutions:**
1. Verify `reporting.outputPath` directory is writable
2. Check disk space availability
3. Ensure tests complete execution (not interrupted)
4. Review logs for specific error messages

---

## Example Configurations

### Minimal Configuration (Fast Execution)

```json
{
  "baseUrl": "https://en.wikipedia.org",
  "browser": {
    "browserType": "chromium",
    "headless": true,
    "timeout": 15000,
    "viewport": { "width": 1280, "height": 720 }
  },
  "reporting": {
    "outputPath": "./Reports",
    "captureScreenshots": false,
    "captureVideo": false
  },
  "logging": {
    "logLevel": "Warning"
  }
}
```

### Comprehensive Configuration (Full Debugging)

```json
{
  "baseUrl": "https://en.wikipedia.org",
  "browser": {
    "browserType": "chromium",
    "headless": false,
    "timeout": 60000,
    "viewport": { "width": 1920, "height": 1080 }
  },
  "reporting": {
    "outputPath": "./Reports",
    "captureScreenshots": true,
    "captureVideo": true
  },
  "mediaWiki": {
    "apiEndpoint": "https://en.wikipedia.org/w/api.php",
    "timeout": 30000
  },
  "logging": {
    "logLevel": "Verbose",
    "logFilePath": "./Reports/detailed-execution.log"
  }
}
```

### CI/CD Configuration (Automated Testing)

```json
{
  "baseUrl": "https://en.wikipedia.org",
  "browser": {
    "browserType": "chromium",
    "headless": true,
    "timeout": 45000,
    "viewport": { "width": 1920, "height": 1080 }
  },
  "reporting": {
    "outputPath": "./Reports",
    "captureScreenshots": true,
    "captureVideo": false
  },
  "mediaWiki": {
    "apiEndpoint": "https://en.wikipedia.org/w/api.php",
    "timeout": 15000
  },
  "logging": {
    "logLevel": "Information",
    "logFilePath": "./Reports/ci-execution.log"
  }
}
```

---

## Additional Resources

- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [NUnit Configuration](https://docs.nunit.org/)
- [Serilog Configuration](https://github.com/serilog/serilog/wiki/Configuration-Basics)
- [JSON Schema Validation](https://www.jsonschemavalidator.net/)
