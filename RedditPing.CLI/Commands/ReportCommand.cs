using System.CommandLine;

namespace RedditPing.CLI.Commands
{
    public partial class CommandBuilder
    {
        public Command BuildReportCommand()
        {
            var postCommand = new Command("report", "Create Reddit report with trending posts");

            postCommand.AddCommand(GenerateReportCommand());
            postCommand.AddCommand(DeleteReportDataCommand());

            return postCommand;
        }
        public Command GenerateReportCommand()
        {
            var subCommand = new Command("generate", "Generate a report with trending Reddit posts.");

            subCommand.SetHandler(h =>
            {
                try
                {
                    var data = _dataStoreService.LoadReportInfo();

                    if (data == null || data.SubredditData == null || !data.SubredditData.Any())
                    {
                        Console.WriteLine("No data available to generate the report.");
                        return;
                    }

                    _reportService.GenerateReport(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            return subCommand;
        }

        public Command DeleteReportDataCommand()
        {
            var subCommand = new Command("delete-data", "Delete existing data with Reddit posts for report generation.");

            subCommand.SetHandler(h =>
            {
                try
                {
                    var dataResponse = _dataStoreService.DeleteData();

                    if (dataResponse)
                        Console.WriteLine("All data for report generation is deleted.");
                    else
                        Console.WriteLine("Failed to delete data for report generation.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            return subCommand;
        }
    }
}
