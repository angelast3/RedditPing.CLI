using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using RedditPing.CLI.Commands;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Services;
using RedditPing.CLI.Services.Interfaces;

#nullable disable
namespace RedditPing.CLI;
public class Program
{
    public static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

        var serviceCollection = new ServiceCollection()
            .Configure<ConfigurationOptions>(configuration.GetSection("Configuration"))
            .AddSingleton<IApiClient, ApiClient>()
            .AddSingleton<IAuthenticationTokenService, AuthenticationTokenService>()
            .AddTransient<IDataStoreService, DataStoreService>()
            .AddSingleton<IReportService, ReportService>()
            .AddSingleton<ISchedulerService, SchedulerService>()
            .AddLogging(builder =>
            {
                builder.AddConsole(); // Add console logging provider here
            });

        serviceCollection.AddHttpClient("RedditApi", client =>  // AddHttpClient BEFORE BuildServiceProvider
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Resolve the services
        var apiClient = serviceProvider.GetService<IApiClient>();
        var dataStoreService = serviceProvider.GetService<IDataStoreService>();
        var logger = serviceProvider.GetService<ILogger<CommandBuilder>>();

        // Define the root command
        var rootCommand = new CommandBuilder(apiClient, dataStoreService, logger).BuildRootCommand();

        if (args.Length > 0 && args.Any(x => x.Equals("exit", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Close application.");
            return;
        }
        else if (args.Length > 0)
        {
            // Parse the command and arguments
            string command = args[0];
            string[] commandArgs = args.Length > 0 ? args : [];
            await rootCommand.InvokeAsync(commandArgs);
        }

        while (true)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            var commandArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandArgs.Length == 1 && commandArgs[0].Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Closing application.");
                return;
            }
            // Run the CLI
            await rootCommand.InvokeAsync(commandArgs);
        }
    }
}