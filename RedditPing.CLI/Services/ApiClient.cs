using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Services;

public class ApiClient : IApiClient
{
    private readonly ConfigurationOptions _config;
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationTokenService _authenticationTokenService;

    public ApiClient(IOptions<ConfigurationOptions> options, IAuthenticationTokenService authenticationTokenService, HttpClient httpClient)
    {
        _config = options.Value;
        _httpClient = httpClient;
        _authenticationTokenService = authenticationTokenService;
    }

    public async Task<JsonElement> GetAsync(string url)
    {
        try
        {
            var accessToken = await _authenticationTokenService.GetAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Access token is invalid or expired.");
            }

            // Set up request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.UserAgent.ParseAdd(@"RedditPing.CLI/1.0.0 (by /u/Possible_Exit334)");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse ?? "");
            var responseList = jsonDoc.RootElement.GetProperty("data").GetProperty("children");

            return responseList;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching posts: {ex.Message}");
            return JsonDocument.Parse("{}").RootElement;
        }
    }


    public async Task<IEnumerable<string>> GetAsyncSimulate(string apiPath)
    {
        var token = await _authenticationTokenService.GetAccessToken();

        //var posts = await GetSubredditPostsWithAuth("technology", token);

        if (apiPath.StartsWith("/post")) {
                        // Simulate Reddit API call (you'll replace this with actual API calls)
            await Task.Delay(500); // Simulate network delay
            return new List<string> { "Post 1", "Post 2" }; // Dummy posts
        }

        return new List<string>();
    }
}

