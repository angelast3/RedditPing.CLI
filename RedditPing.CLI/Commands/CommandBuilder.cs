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
        //private readonly ConfigurationOptions _config;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="apiClient"></param>
        public CommandBuilder(IApiClient apiClient, IDataStoreService dataStoreService, ILogger<CommandBuilder> logger)
        {
            //_config = options.Value;
            _apiClient = apiClient;
            _dataStoreService = dataStoreService;
            _logger = logger;
        }

        public RootCommand BuildRootCommand()
        {
            _logger.LogInformation($"Creating commands...");

            var rootCommand = new RootCommand
            {
                Name = AppConstants.CLIName,
                Description = "A CLI app for interacting with Reddit"
            };

            _logger.LogInformation($"Root command '{rootCommand.Name}' created.");

            //Add commands
            rootCommand.AddCommand(BuildPostCommand());
            rootCommand.AddCommand(BuildSubredditCommand());
            rootCommand.AddCommand(BuildReportCommand());

            _logger.LogInformation($"All commands added to root command.");

            return rootCommand;
        }
    }
}
