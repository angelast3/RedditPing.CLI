namespace RedditPing.CLI.Models;

#pragma warning disable CS8618
public class ReportInfo
{
    public string Date { get; set; } //time report
    public List<SubredditData> SubredditData { get; set; }
}
