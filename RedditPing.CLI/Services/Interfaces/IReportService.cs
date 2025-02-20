namespace RedditPing.CLI.Services.Interfaces
{
    public interface IReportService
    {
        public void GenerateDailyReport();
        public void GenerateMorningReport();
    }
}
