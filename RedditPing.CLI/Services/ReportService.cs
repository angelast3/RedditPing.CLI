using RedditPing.CLI.Constants;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Services
{
    public class ReportService : IReportService
    {
        private readonly IDataStoreService _storage;

        public ReportService(IDataStoreService storage)
        {
            _storage = storage;
        }

        public void GenerateDailyReport()
        {
            var data = _storage.LoadReportInfo();
            var yesterday = DateTime.Now.AddDays(-1).Date.ToString(AppConstants.DateFormat);

            if (data.Date.Equals(yesterday))
            {
                //Console.WriteLine($"Generating Daily Report for {yesterday}...");
                //foreach (var post in data[yesterday])
                //{
                //    Console.WriteLine($"Post: {post.Title}, Upvotes: {post.Upvotes}, Comments: {post.Comments}");
                //}
            }
            else
            {
                Console.WriteLine("No data available for the previous day.");
            }
        }

        public void GenerateMorningReport()
        {
            Console.WriteLine("Generating Morning Snapshot...");
            // Fetch current data and display it (simpler format)
        }
    }
}
