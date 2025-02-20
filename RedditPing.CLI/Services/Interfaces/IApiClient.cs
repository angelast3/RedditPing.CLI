using System.Text.Json;

namespace RedditPing.CLI.Services.Interfaces
{
    public interface IApiClient
    {
        public Task<IEnumerable<string>> GetAsyncSimulate(string apiPath);
        public Task<JsonElement> GetAsync(string subreddit);
    }
}
