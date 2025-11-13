# Setup Instructions

This document provides step-by-step instructions for setting up the Playwright C# Automation Framework.

## Prerequisites Check

Before proceeding, verify you have the required tools installed:

### 1. Check .NET SDK Installation

```bash
dotnet --version
```

Expected output: `8.0.x` or higher

If not installed:
- **macOS**: `brew install dotnet@8`
- **Windows**: Download from https://dotnet.microsoft.com/download/dotnet/8.0
- **Linux**: Follow instructions at https://learn.microsoft.com/en-us/dotnet/core/install/linux

### 2. Check PowerShell Installation

```bash
pwsh --version
```

Expected output: `PowerShell 7.x.x` or higher

If not installed:
- **macOS**: `brew install --cask powershell`
- **Windows**: PowerShell is pre-installed
- **Linux**: Follow instructions at https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-linux

### 3. Check Make Installation

```bash
make --version
```

Expected output: `GNU Make x.x` or similar

If not installed:
- **macOS**: `xcode-select --install`
- **Windows**: Install via Chocolatey `choco install make` or use WSL
- **Linux**: `sudo apt-get install build-essential`

## Installation Steps

Once all prerequisites are installed, run:

```bash
# Step 1: Restore NuGet packages
dotnet restore

# Step 2: Build the project
dotnet build

# Step 3: Install Playwright browsers
pwsh bin/Debug/net8.0/playwright.ps1 install
```

Or simply use the Makefile:

```bash
make install
```

## Verification

After installation, verify everything is set up correctly:

```bash
# Check that the project builds
make build

# Run a simple test to verify Playwright is working
make test
```

## Common Issues

### Issue: "dotnet: command not found"
**Solution**: Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

### Issue: "pwsh: command not found"
**Solution**: Install PowerShell Core from https://github.com/PowerShell/PowerShell

### Issue: Playwright browsers not installing
**Solution**: 
1. Ensure PowerShell is installed
2. Run manually: `pwsh bin/Debug/net8.0/playwright.ps1 install`
3. Check internet connectivity

### Issue: Permission denied on macOS/Linux
**Solution**: 
```bash
chmod +x bin/Debug/net8.0/playwright.ps1
```

## Next Steps

Once setup is complete:

1. Review `appsettings.json` for configuration options
2. Read `README.md` for usage instructions
3. Run `make test` to execute all tests
4. Check `Reports/` directory for test results

## Manual Browser Installation

If the automated installation fails, you can install Playwright browsers manually:

```bash
# After building the project
cd bin/Debug/net8.0
pwsh playwright.ps1 install chromium
pwsh playwright.ps1 install firefox
pwsh playwright.ps1 install webkit
```

## Environment Variables (Optional)

You can set these environment variables for additional configuration:

```bash
# Set browser download path
export PLAYWRIGHT_BROWSERS_PATH=$HOME/.playwright

# Enable debug logging
export DEBUG=pw:api
```

## Support

For issues or questions:
1. Check the troubleshooting section in README.md
2. Review Playwright documentation: https://playwright.dev/dotnet/
3. Check NUnit documentation: https://docs.nunit.org/
