using System.Text.Json;

namespace RedditPing.CLI.Services.Interfaces
{
    public interface IApiClient
    {
        public Task<JsonElement> GetListAsync(string subreddit);
        public Task<JsonElement> GetObjectAsync(string subreddit);
    }
}
