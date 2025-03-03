namespace RedditPing.CLI.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<string?> GetAccessToken();
        public Task SetAccessTokenAsync();
    }
}
