
namespace RedditPing.CLI.Models;

#pragma warning disable CS8618
public class SubredditData
{
    public SubReddit Subreddit { get; set; }
    public Dictionary<string, List<RedditPost>> PostsByTimestamp { get; set; }

}
