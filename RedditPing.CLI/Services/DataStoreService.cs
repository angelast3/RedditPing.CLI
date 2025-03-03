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
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File does not exist. Returning default ReportInfo.");
                    return new ReportInfo();
                }

                var json = File.ReadAllText(filePath);
                var reportInfo = JsonConvert.DeserializeObject<ReportInfo>(json);

                return reportInfo ?? new ReportInfo();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error while reading file: {ex.Message}");
                return new ReportInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return new ReportInfo();
            }
        }

        public bool DeleteData()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File does not exist.");
                    return true;
                }

                var emptyReportInfo = new ReportInfo();
                var json = JsonConvert.SerializeObject(emptyReportInfo, Formatting.Indented);

                File.WriteAllText(filePath, json);
                Console.WriteLine("Data cleared successfully.");

                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error while deleting file data: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }

        public void UpdateSubreddits(List<SubReddit> subreddits)
        {
            var reportInfo = LoadReportInfo();
            // Create new report info
            if (reportInfo == null || reportInfo.SubredditData == null || reportInfo.SubredditData.Count < 1)
            {
                reportInfo = new ReportInfo
                {
                    Date = DateTime.UtcNow.Date.ToString(AppConstants.DateFormat),
                    SubredditData = new List<SubredditData>() // Create empty list instead of []
                };
            }
            // If there is already valid data, do not override
            else if (reportInfo.SubredditData.Count > 0)
            {
                return;
            }

            // Add the new subreddits
            foreach (var subreddit in subreddits)
            {
                // Avoid adding duplicate subreddits
                if (!reportInfo.SubredditData.Any(s => s.Subreddit.Id == subreddit.Id))
                {
                    reportInfo.SubredditData.Add(new SubredditData
                    {
                        Subreddit = subreddit,
                        PostsByTimestamp = new Dictionary<string, List<RedditPost>>()
                    });
                }
            }

            SaveReportInfo(reportInfo);
        }

        public void UpdatePosts(List<SubredditReport> subredditPosts)
        {
            var reportInfo = LoadReportInfo();
            var now = DateTime.UtcNow.ToString(AppConstants.DateTimeFormat);

            foreach (var subreddit in reportInfo.SubredditData)
            {
                var sr = subredditPosts.FirstOrDefault(x => x.Id.Equals(subreddit.Subreddit.Id));
                if (sr != null)
                {
                    // If PostsByTimestamp is null, initialize it
                    if (subreddit.PostsByTimestamp == null)
                    {
                        subreddit.PostsByTimestamp = new Dictionary<string, List<RedditPost>>();
                    }

                    // Add posts for the current timestamp
                    subreddit.PostsByTimestamp[now] = sr.Posts;
                }
            }

            SaveReportInfo(reportInfo);
        }

        private void SaveReportInfo(ReportInfo reportInfo)
        {
            var json = JsonConvert.SerializeObject(reportInfo, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
