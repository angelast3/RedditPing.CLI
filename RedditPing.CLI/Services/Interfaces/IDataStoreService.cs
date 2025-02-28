
using RedditPing.CLI.Models;

namespace RedditPing.CLI.Services.Interfaces
{
    public interface IDataStoreService
    {
        public ReportInfo LoadReportInfo();
        public void UpdateSubreddits(List<SubReddit> subreddits);
        public void UpdatePosts(List<SubredditReport> subredditPosts);
        public bool DeleteData();
    }
}
