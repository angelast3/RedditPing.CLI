namespace RedditPing.CLI.Models;

#pragma warning disable CS8618
public class TokenInfo
{
    public string? AccessToken { set; get; }
    public int ExpiresIn { set; get; }
    public DateTime ExpirationTime { set; get; }
}

