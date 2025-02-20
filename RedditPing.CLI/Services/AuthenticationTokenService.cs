using System.Net.Http.Headers;
using System.Text.Json;
using RedditPing.CLI.Services.Interfaces;
using Microsoft.Extensions.Options;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Models;
using RedditPing.CLI.Constants;

namespace RedditPing.CLI.Services
{
    public class AuthenticationTokenService(IOptions<ConfigurationOptions> options) : IAuthenticationTokenService
    {
        private TokenInfo _accessToken = new();
        private readonly ConfigurationOptions _config = options.Value;
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<string?> GetAccessToken()
        {
            //return "eyJhbGciOiJSUzI1NiIsImtpZCI6IlNIQTI1NjpzS3dsMnlsV0VtMjVmcXhwTU40cWY4MXE2OWFFdWFyMnpLMUdhVGxjdWNZIiwidHlwIjoiSldUIn0.eyJzdWIiOiJsb2lkIiwiZXhwIjoxNzM2MzY1OTgzLjgzNjA5NywiaWF0IjoxNzM2Mjc5NTgzLjgzNjA5NywianRpIjoiOGJtWmVRSTItX0dneXJwSDI3REJCa3BleFVqNEZnIiwiY2lkIjoiazZ6RkJlYU9iYl9DSFk3X0IzRVBnUSIsImxpZCI6InQyXzFnbzc4YThjdXIiLCJsY2EiOjE3MzYyNzk1ODM4MjEsInNjcCI6ImVKeUtWdEpTaWdVRUFBRF9fd056QVNjIiwiZmxvIjo2fQ.lMwX0jHoP5isTmfr6pgz22lHSaI4H3rhzPhJzIxGg3UXXqGEBBaVnwlKTi9y5ZVYrvNfQfDsy2xIKNxdO9Ro94GUw0scyosSrSawrqP7pr64tUMsphmHUefy-SEXkf4xK5WodgO2_gHgDros3iCPDjnaXVSZhMoqo5aJH7sShkvUIcfPkRwrIZvoRO9a4mWH_Ul9p-uUYxnjGzj_j74c80QDXBIRREmJOxVIcZ3eyCAR-x62ymD-PeuL8HxiX9DgoGyAE1z5aQ8HnI48TY53THo6lSwVGCwJDWqlNQT3954veagHwo-xgrnMpE0Nf0HKRo41cu8xzXZWrn90l0jcrg";
            if (string.IsNullOrWhiteSpace(_accessToken.AccessToken) || (_accessToken.ExpirationTime < DateTime.UtcNow.AddMinutes(5)))
                await SetAccessTokenAsync();
            else if (!string.IsNullOrWhiteSpace(_accessToken.AccessToken) && _accessToken.ExpirationTime < DateTime.UtcNow)
                _accessToken = new TokenInfo();

             return _accessToken.AccessToken;
        }

        public async Task SetAccessTokenAsync()
        {
            var clientId = _config.ClientConfig?.ClientId;
            var clientSecret = _config.ClientConfig?.ClientSecret;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException("ClientId or ClientSecret is missing in configuration.");
            }

            // Build the request
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
            request.Headers.UserAgent.ParseAdd(@"RedditPing.CLI/1.0.0 (by /u/Possible_Exit334)");

            var response = await _httpClient.SendAsync(request);

            // Ensure the response was successful
            response.EnsureSuccessStatusCode();

            // Parse the response content
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse);

            var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = jsonDoc.RootElement.GetProperty("expires_in").GetInt32();

            _accessToken.AccessToken = accessToken;
            _accessToken.ExpiresIn = expiresIn;
            _accessToken.ExpirationTime = DateTime.UtcNow.AddSeconds(expiresIn);

            // Extract the access token
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("Access token could not be retrieved.");
            }
        }
    }
}
