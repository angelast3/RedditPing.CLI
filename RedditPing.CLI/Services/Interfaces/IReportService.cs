using QuestPDF.Infrastructure;
using RedditPing.CLI.Models;

namespace RedditPing.CLI.Services.Interfaces
{
    public interface IReportService
    {
        public void GenerateReport(ReportInfo reportInfo);
        public string GenerateLineChart(SubredditData subredditData);
        public void ComposePostCard(IContainer container, RedditPost post, double averageScore, bool isTopPost);
    }
}
