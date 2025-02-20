namespace RedditPing.CLI.Models;

#pragma warning disable CS8618
public class RedditPost
{
    public string id { get; set; }
    public string title { get; set; }
    public string author { get; set; }
    public string subreddit { get; set; }
    public double upvote_ratio { get; set; }
    public int ups { get; set; }
    public int downs { get; set; }
    public int score { get; set; }
    public string link_flair_text { get; set; }
    public bool over_18 { get; set; }
    public int num_comments { get; set; }
    public string url { get; set; }
}
