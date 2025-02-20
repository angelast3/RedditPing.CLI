using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IReportService _reportService;

        public SchedulerService(IReportService reportService)
        {
            _reportService = reportService;
        }

        public void StartScheduler()
        {
            Console.WriteLine("Checking if reports need to be generated...");

            // Check if it's the first start of the day
            var lastRunFile = "lastrun.txt";
            var lastRun = File.Exists(lastRunFile) ? DateTime.Parse(File.ReadAllText(lastRunFile)) : DateTime.MinValue;

            if (lastRun.Date != DateTime.Now.Date)
            {
                _reportService.GenerateDailyReport();
                _reportService.GenerateMorningReport();
                File.WriteAllText(lastRunFile, DateTime.Now.ToString());
            }

            // Continue with regular fetching logic
            Console.WriteLine("Scheduler initialized. Waiting for data fetch triggers...");
        }
    }
}
