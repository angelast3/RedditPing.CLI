using Newtonsoft.Json;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Models;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Services
{
    public class DataStoreService : IDataStoreService
    {
        private readonly string filePath = $"reportData.json";

        public ReportInfo LoadReportInfo()
        {
            if (!File.Exists(filePath)) return new ReportInfo();
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ReportInfo>(json) ?? new ReportInfo();
        }

        public void UpdateSubreddits(List<SubReddit> subreddits)
        {
            var reportInfo = LoadReportInfo();
            // create new report info
            if(reportInfo == null || reportInfo.SubredditData == null || reportInfo.SubredditData.Count < 1)
            {
                reportInfo = new ReportInfo(); 
                reportInfo.Date = DateTime.UtcNow.Date.ToString(AppConstants.DateFormat);
                reportInfo.SubredditData = [];
            }
            //if there are already applied do not override existing
            else
                return;

            // Create new subreddits
            var data = new Dictionary<string, List<SubredditReport>>();

            foreach (var subreddit in subreddits)
            {
                reportInfo.SubredditData.Add(new SubredditData
                {
                    Subreddit = subreddit,
                    PostsByTimestamp = new Dictionary<string, List<RedditPost>>()

                });
            };

            var json = JsonConvert.SerializeObject(reportInfo, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void UpdatePosts(List<SubredditReport> subredditPosts)
        {
            var reportInfo = LoadReportInfo();
            var now = DateTime.UtcNow.ToString(AppConstants.DateTimeFormat);

            foreach (var subreddit in reportInfo.SubredditData)
            {

                var sr = subredditPosts.FirstOrDefault(x => x.Id.Equals(subreddit.Subreddit.id));
                if (sr != null)
                {
                    if (subreddit.PostsByTimestamp != null)
                        subreddit.PostsByTimestamp.Add(now, sr.Posts);
                    else
                        subreddit.PostsByTimestamp = new Dictionary<string, List<RedditPost>>
                        {
                            { now, sr.Posts }
                        };
                }
            }

            var json = JsonConvert.SerializeObject(reportInfo, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
