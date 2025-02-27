using System;
using System.Collections.Generic;
using System.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Commands
{
    public partial class CommandBuilder
    {
        private readonly IApiClient _apiClient;
        private readonly IDataStoreService _dataStoreService;
        private readonly ILogger<CommandBuilder> _logger; // Add logger
        private readonly ConfigurationOptions _config;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="apiClient"></param>
        public CommandBuilder(IApiClient apiClient, IDataStoreService dataStoreService, ILogger<CommandBuilder> logger, IOptions<ConfigurationOptions> options)
        {
            _apiClient = apiClient;
            _dataStoreService = dataStoreService;
            _logger = logger;
            _config = options.Value;
        }

        public RootCommand BuildRootCommand()
        {
            Console.WriteLine(_config.ClientConfig.ClientId);

            Console.WriteLine(_config.TrackingSettings.Subreddits[0]);
            var rootCommand = new RootCommand
            {
                Name = AppConstants.CLIName,
                Description = "A CLI app for interacting with Reddit"
            };

            //Add commands
            rootCommand.AddCommand(BuildPostCommand());
            rootCommand.AddCommand(BuildSubredditCommand());
            rootCommand.AddCommand(BuildReportCommand());

            return rootCommand;
        }
    }
}
