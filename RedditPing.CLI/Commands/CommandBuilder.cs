using System.CommandLine;
using Microsoft.Extensions.Logging;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Commands
{
    public partial class CommandBuilder
    {
        private readonly IApiClient _apiClient;
        private readonly IDataStoreService _dataStoreService;
        private readonly ILogger<CommandBuilder> _logger;
        private readonly ConfigurationOptions _config;
        private readonly IReportService _reportService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="apiClient"></param>
        public CommandBuilder(IApiClient apiClient, IDataStoreService dataStoreService, ILogger<CommandBuilder> logger,
            ConfigurationOptions options, IReportService reportService)
        {
            _apiClient = apiClient;
            _dataStoreService = dataStoreService;
            _logger = logger;
            _config = options;
            _reportService = reportService;
        }

        /// <summary>
        /// Builds the root Command object with all sub-commands.
        /// </summary>
        /// <returns>Main RootCommand command.</returns>
        public RootCommand BuildRootCommand()
        {
           
            var rootCommand = new RootCommand
            {
                Name = AppConstants.CLIName,
                Description = "A CLI app for fetching and processing data from Reddit"
            };

            //Add commands
            rootCommand.AddCommand(BuildPostCommand());
            rootCommand.AddCommand(BuildSubredditCommand());
            rootCommand.AddCommand(BuildReportCommand());

            return rootCommand;
        }
    }
}
