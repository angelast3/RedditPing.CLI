using System;
using System.Collections.Generic;
using System.CommandLine;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Commands
{
    public partial class CommandBuilder
    {
        private readonly IApiClient _apiClient;
        private readonly IDataStoreService _dataStoreService;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="apiClient"></param>
        public CommandBuilder(IApiClient apiClient, IDataStoreService dataStoreService)
        {
            _apiClient = apiClient;
            _dataStoreService = dataStoreService;
        }

        public RootCommand BuildRootCommand()
        {
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
