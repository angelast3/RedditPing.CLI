using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Models;
using RedditPing.CLI.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

public class AuthenticationTokenService : IAuthenticationTokenService
{
    private TokenInfo _accessToken = new();
    private readonly ConfigurationOptions _config;
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly ILogger<AuthenticationTokenService> _logger;

    public AuthenticationTokenService(IOptions<ConfigurationOptions> options, ILogger<AuthenticationTokenService> logger)
    {
        _config = options.Value;
        _logger = logger;
    }

    public async Task<string?> GetAccessToken()
    {
        if (string.IsNullOrWhiteSpace(_accessToken.AccessToken) || (_accessToken.ExpirationTime < DateTime.UtcNow.AddMinutes(5)))
            await SetAccessTokenAsync();
        else if (!string.IsNullOrWhiteSpace(_accessToken.AccessToken) && _accessToken.ExpirationTime < DateTime.UtcNow)
            _accessToken = new TokenInfo();

        return _accessToken.AccessToken;
    }

    public async Task SetAccessTokenAsync()
    {
        try
        {
            _logger.LogDebug("Auth Service: Starting access token retrieval...");

            var clientId = _config.ClientConfig?.ClientId;
            var clientSecret = _config.ClientConfig?.ClientSecret;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                _logger.LogError("Auth Service: ClientId or ClientSecret is missing in configuration.");
                throw new InvalidOperationException("ClientId or ClientSecret is missing in configuration.");
            }

            _logger.LogDebug("Auth Service: ClientId and ClientSecret successfully retrieved from configuration.");

            var request = new HttpRequestMessage(HttpMethod.Post, AppConstants.TokenUrl)
            {
                Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                ])
            };

            // Add required headers
            var authHeader = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Headers.UserAgent.ParseAdd("RedditPing.CLI/1.0.0 (by /u/Possible_Exit334)");

            _logger.LogDebug("Auth Service: Sending token request...");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError("Auth Service: Failed to retrieve access token. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorMessage);
                response.EnsureSuccessStatusCode();
            }

            _logger.LogDebug("Auth Service: Token request successful. Parsing response...");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);

            if (!jsonDoc.RootElement.TryGetProperty("access_token", out var accessTokenElement) ||
                !jsonDoc.RootElement.TryGetProperty("expires_in", out var expiresInElement))
            {
                _logger.LogError("Auth Service: Access token response is missing required fields: {Response}", jsonResponse);
                throw new InvalidOperationException("Access token response is missing required fields.");
            }

            var accessToken = accessTokenElement.GetString();
            var expiresIn = expiresInElement.GetInt32();

            _accessToken.AccessToken = accessToken;
            _accessToken.ExpiresIn = expiresIn;
            _accessToken.ExpirationTime = DateTime.UtcNow.AddSeconds(expiresIn);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Auth Service: Access token could not be retrieved.");
                throw new InvalidOperationException("Access token could not be retrieved.");
            }

            _logger.LogDebug("Auth Service: Access token successfully retrieved. Expires in {ExpiresIn} seconds.", expiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError("Auth Service: An error occurred while retrieving the access token. Error: {Error}", ex.Message);
            throw;
        }
    }
}
