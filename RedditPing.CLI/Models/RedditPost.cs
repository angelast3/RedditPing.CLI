using System.Text.Json.Serialization;

namespace RedditPing.CLI.Models;
#pragma warning disable CS8618

public class RedditPost
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("subreddit")]
    public string Subreddit { get; set; }

    [JsonPropertyName("upvote_ratio")]
    public double UpvoteRatio { get; set; }

    [JsonPropertyName("ups")]
    public int Ups { get; set; }

    [JsonPropertyName("downs")]
    public int Downs { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("link_flair_text")]
    public string LinkFlairText { get; set; }

    [JsonPropertyName("over_18")]
    public bool Over18 { get; set; }

    [JsonPropertyName("num_comments")]
    public int NumComments { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
