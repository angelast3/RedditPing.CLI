using System.Net.Http.Headers;
using System.Text.Json;
using RedditPing.CLI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace RedditPing.CLI.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationTokenService;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(IAuthenticationService authenticationTokenService, HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _authenticationTokenService = authenticationTokenService;
        _logger = logger;
    }

    public async Task<JsonElement> GetListAsync(string url)
    {
        try
        {
            _logger.LogInformation("Api Client: Starting the GET request for URL: {Url}", url);

            var accessToken = await _authenticationTokenService.GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Api Client: Access token is invalid or expired.");
                throw new InvalidOperationException("Access token is invalid or expired.");
            }

            _logger.LogDebug("Api Client: Access token retrieved successfully.");

            // Set up request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.UserAgent.ParseAdd(@"RedditPing.CLI/1.0.0 (by /u/Possible_Exit334)");

            _logger.LogInformation("Api Client: Sending GET request.");

            var response = await _httpClient.SendAsync(request);
            _logger.LogInformation("Api Client: Received response with status code: {StatusCode}", response.StatusCode);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse ?? "");

            _logger.LogInformation("Api Client: Parsed JSON response successfully.");

            var responseList = jsonDoc.RootElement.GetProperty("data").GetProperty("children");

            return responseList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Api Client: Error fetching data from URL: {Url}", url);
            Console.WriteLine($"Error fetching posts: {ex.Message}");
            return JsonDocument.Parse("{}").RootElement;
        }
    }

    public async Task<JsonElement> GetObjectAsync(string url)
    {
        try
        {
            _logger.LogInformation("Api Client: Starting the GET request for URL: {Url}", url);

            var accessToken = await _authenticationTokenService.GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Api Client: Access token is invalid or expired.");
                throw new InvalidOperationException("Access token is invalid or expired.");
            }

            _logger.LogDebug("Api Client: Access token retrieved successfully.");

            // Set up request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.UserAgent.ParseAdd(@"RedditPing.CLI/1.0.0 (by /u/Possible_Exit334)");

            _logger.LogInformation("Api Client: Sending GET request.");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse ?? "");

            var responseObj = jsonDoc.RootElement.GetProperty("data");

            return responseObj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Api Client: Error fetching data from URL: {Url}", url);
            Console.WriteLine($"Error fetching posts: {ex.Message}");
            return JsonDocument.Parse("{}").RootElement;
        }
    }
}
