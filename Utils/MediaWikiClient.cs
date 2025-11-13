using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using RestSharp;
using Serilog;
using PlaywrightAutomation.Models;

namespace PlaywrightAutomation.Utils;

public class MediaWikiClient
{
    private readonly RestClient _restClient;
    private readonly AsyncRetryPolicy<RestResponse> _retryPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;
    private readonly ILogger _logger;
    private const string ApiEndpoint = "https://en.wikipedia.org/w/api.php";

    public MediaWikiClient(ILogger logger)
    {
        _logger = logger;
        
        var options = new RestClientOptions(ApiEndpoint)
        {
            MaxTimeout = 10000
        };
        _restClient = new RestClient(options);

        // Configure Polly retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<RestResponse>(r => !r.IsSuccessful)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.Warning(
                        "API call failed. Retry {RetryCount} after {Delay}s. Status: {StatusCode}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        outcome.Result?.StatusCode);
                });

        // Configure timeout policy
        _timeoutPolicy = Policy.TimeoutAsync(10);
    }

    public async Task<string> GetPageSection(string pageTitle, string sectionTitle)
    {
        try
        {
            _logger.Information("Retrieving section '{SectionTitle}' from page '{PageTitle}'", sectionTitle, pageTitle);

            // First, find the section index
            var sectionIndex = await FindSectionIndex(pageTitle, sectionTitle);
            
            if (sectionIndex == -1)
            {
                _logger.Error("Section '{SectionTitle}' not found in page '{PageTitle}'", sectionTitle, pageTitle);
                throw new InvalidOperationException($"Section '{sectionTitle}' not found in page '{pageTitle}'");
            }

            // Then, parse the section content
            var content = await ParseSectionContent(pageTitle, sectionIndex);
            
            _logger.Information("Successfully retrieved section content. Length: {Length} characters", content.Length);
            return content;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve section '{SectionTitle}' from page '{PageTitle}'", sectionTitle, pageTitle);
            throw;
        }
    }

    private async Task<int> FindSectionIndex(string pageTitle, string sectionTitle)
    {
        try
        {
            var request = new RestRequest()
                .AddParameter("action", "parse")
                .AddParameter("page", pageTitle)
                .AddParameter("prop", "sections")
                .AddParameter("format", "json");

            _logger.Debug("Finding section index for '{SectionTitle}' in page '{PageTitle}'", sectionTitle, pageTitle);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _timeoutPolicy.ExecuteAsync(async ct =>
                    await _restClient.ExecuteGetAsync(request, ct), CancellationToken.None));

            if (!response.IsSuccessful || response.Content == null)
            {
                _logger.Error("Failed to retrieve sections. Status: {StatusCode}, Error: {ErrorMessage}",
                    response.StatusCode, response.ErrorMessage);
                throw new HttpRequestException($"API request failed: {response.ErrorMessage}");
            }

            var apiResponse = JsonSerializer.Deserialize<MediaWikiResponse>(response.Content);
            
            if (apiResponse?.Parse?.Sections == null)
            {
                _logger.Error("Invalid API response structure");
                throw new InvalidOperationException("Invalid API response structure");
            }

            // Find the section by title (case-insensitive)
            var section = apiResponse.Parse.Sections
                .FirstOrDefault(s => s.Line.Equals(sectionTitle, StringComparison.OrdinalIgnoreCase));

            if (section == null)
            {
                _logger.Warning("Section '{SectionTitle}' not found. Available sections: {Sections}",
                    sectionTitle, string.Join(", ", apiResponse.Parse.Sections.Select(s => s.Line)));
                return -1;
            }

            var sectionIndex = int.Parse(section.Index);
            _logger.Debug("Found section '{SectionTitle}' at index {Index}", sectionTitle, sectionIndex);
            return sectionIndex;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error finding section index for '{SectionTitle}'", sectionTitle);
            throw;
        }
    }

    private async Task<string> ParseSectionContent(string pageTitle, int sectionIndex)
    {
        try
        {
            var request = new RestRequest()
                .AddParameter("action", "parse")
                .AddParameter("page", pageTitle)
                .AddParameter("section", sectionIndex)
                .AddParameter("prop", "text")
                .AddParameter("format", "json");

            _logger.Debug("Parsing section content for page '{PageTitle}', section index {SectionIndex}",
                pageTitle, sectionIndex);

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _timeoutPolicy.ExecuteAsync(async ct =>
                    await _restClient.ExecuteGetAsync(request, ct), CancellationToken.None));

            if (!response.IsSuccessful || response.Content == null)
            {
                _logger.Error("Failed to parse section content. Status: {StatusCode}, Error: {ErrorMessage}",
                    response.StatusCode, response.ErrorMessage);
                throw new HttpRequestException($"API request failed: {response.ErrorMessage}");
            }

            var apiResponse = JsonSerializer.Deserialize<MediaWikiResponse>(response.Content);
            
            if (apiResponse?.Parse?.Text?.Content == null)
            {
                _logger.Error("Invalid API response structure or empty content");
                throw new InvalidOperationException("Invalid API response structure or empty content");
            }

            _logger.Debug("Successfully parsed section content. Length: {Length} characters",
                apiResponse.Parse.Text.Content.Length);
            
            return apiResponse.Parse.Text.Content;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error parsing section content for page '{PageTitle}', section {SectionIndex}",
                pageTitle, sectionIndex);
            throw;
        }
    }
}
