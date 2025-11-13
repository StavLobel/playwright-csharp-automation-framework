.PHONY: install build test test-ui test-api clean report help verify-deps

# Default target
help:
	@echo "Playwright C# Automation Framework - Available Commands:"
	@echo ""
	@echo "  make install    - Restore NuGet packages and install Playwright browsers"
	@echo "  make build      - Compile the project"
	@echo "  make test       - Run all tests (UI + API) and generate HTML report"
	@echo "  make test-ui    - Run UI tests only"
	@echo "  make test-api   - Run API tests only"
	@echo "  make clean      - Remove build artifacts and reports"
	@echo "  make report     - Open the latest HTML report in browser"
	@echo "  make verify-deps - Verify all required dependencies are installed"
	@echo ""

# Verify dependencies
verify-deps:
	@echo "Verifying dependencies..."
	@command -v dotnet >/dev/null 2>&1 || { echo "Error: .NET SDK is not installed. Please install .NET 8 SDK."; exit 1; }
	@dotnet --version | grep -q "^8\." || { echo "Warning: .NET 8 is recommended. Current version: $$(dotnet --version)"; }
	@command -v pwsh >/dev/null 2>&1 || { echo "Error: PowerShell (pwsh) is not installed. Required for Playwright browser installation."; exit 1; }
	@echo "✓ .NET SDK found: $$(dotnet --version)"
	@echo "✓ PowerShell found: $$(pwsh --version | head -n 1)"
	@echo "All dependencies verified!"

# Install dependencies and Playwright browsers
install: verify-deps
	@echo "Installing dependencies..."
	@dotnet restore || { echo "Error: Failed to restore NuGet packages"; exit 1; }
	@echo "Building project..."
	@dotnet build || { echo "Error: Failed to build project"; exit 1; }
	@echo "Installing Playwright browsers..."
	@pwsh bin/Debug/net8.0/playwright.ps1 install || { echo "Error: Failed to install Playwright browsers"; exit 1; }
	@echo "✓ Installation complete!"

# Build the project
build:
	@echo "Building project..."
	@dotnet build --configuration Release || { echo "Error: Build failed"; exit 1; }
	@echo "✓ Build complete!"

# Run all tests with HTML report generation
test:
	@echo "Running all tests..."
	@mkdir -p TestResults Reports
	@dotnet test --configuration Release \
		--logger "console;verbosity=detailed" \
		--logger "trx;LogFileName=test-results.trx" \
		--logger "html;LogFileName=test-results.html" \
		--results-directory ./TestResults || { echo "Warning: Some tests failed. Check reports for details."; }
	@echo "✓ Tests complete! Reports generated in ./TestResults"
	@echo "  - TRX Report: ./TestResults/test-results.trx"
	@echo "  - HTML Report: ./TestResults/test-results.html"

# Run UI tests only
test-ui:
	@echo "Running UI tests..."
	@mkdir -p TestResults
	@dotnet test --configuration Release \
		--filter "Category=UI" \
		--logger "console;verbosity=detailed" \
		--logger "trx;LogFileName=ui-test-results.trx" \
		--logger "html;LogFileName=ui-test-results.html" \
		--results-directory ./TestResults || { echo "Warning: Some UI tests failed."; }
	@echo "✓ UI tests complete!"

# Run API tests only
test-api:
	@echo "Running API tests..."
	@mkdir -p TestResults
	@dotnet test --configuration Release \
		--filter "Category=API" \
		--logger "console;verbosity=detailed" \
		--logger "trx;LogFileName=api-test-results.trx" \
		--logger "html;LogFileName=api-test-results.html" \
		--results-directory ./TestResults || { echo "Warning: Some API tests failed."; }
	@echo "✓ API tests complete!"

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@dotnet clean
	@rm -rf bin obj TestResults Reports/playwright-report Reports/test-artifacts
	@echo "✓ Clean complete!"

# Open HTML report in browser
report:
	@echo "Opening test report..."
	@if [ -f "TestResults/test-results.html" ]; then \
		open TestResults/test-results.html 2>/dev/null || xdg-open TestResults/test-results.html 2>/dev/null || start TestResults/test-results.html 2>/dev/null || echo "Please open TestResults/test-results.html manually"; \
	elif [ -f "Reports/playwright-report/index.html" ]; then \
		open Reports/playwright-report/index.html 2>/dev/null || xdg-open Reports/playwright-report/index.html 2>/dev/null || start Reports/playwright-report/index.html 2>/dev/null || echo "Please open Reports/playwright-report/index.html manually"; \
	else \
		echo "No report found. Run 'make test' first."; \
	fi
