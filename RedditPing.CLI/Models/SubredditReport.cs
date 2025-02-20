
namespace RedditPing.CLI.Models;

#pragma warning disable CS8618
public class SubredditReport
{
    public string Id { get; set; }
    public List<RedditPost> Posts { get; set; }
}
