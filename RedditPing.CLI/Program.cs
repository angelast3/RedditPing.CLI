using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedditPing.CLI.Commands;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Services;
using RedditPing.CLI.Services.Interfaces;
using Serilog;

#nullable disable
namespace RedditPing.CLI;

public class Program
{
    public static async Task Main(string[] args)
    {
        IConfiguration configuration = BuildConfiguration();

        var subredditsList = configuration.GetSection("Configuration:TrackingSettings:Subreddits").Get<List<string>>();

        // If it's not found or binding fails, try to handle it as a string
        if (subredditsList == null)
        {
            var subredditsString = configuration["Configuration:TrackingSettings:Subreddits"];
            if (!string.IsNullOrWhiteSpace(subredditsString))
            {
                subredditsList = subredditsString
                    .Trim('[', ']')
                    .Split(',')
                    .Select(s => s.Trim().Trim('"'))
                    .ToList();
            }
        }

        var options = new ConfigurationOptions();
        configuration.GetSection("Configuration").Bind(options);

        if (subredditsList != null)
            options.TrackingSettings.Subreddits = subredditsList;
        

        // Register services and resolve dependencies
        var serviceProvider = BuildServiceProvider(configuration);
        var logger = serviceProvider.GetRequiredService<ILogger<CommandBuilder>>();
        var apiClient = serviceProvider.GetService<IApiClient>();
        var dataStoreService = serviceProvider.GetService<IDataStoreService>();
        var reportService = serviceProvider.GetService<IReportService>();

        // Define the root command
        var rootCommand = new CommandBuilder(apiClient, dataStoreService, logger, options, reportService).BuildRootCommand();

        // Check if 'exit' command is passed
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
            return;
        }

#if DEBUG
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

            await rootCommand.InvokeAsync(commandArgs);
        }
#endif
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

        var serviceCollection = new ServiceCollection()
            .Configure<ConfigurationOptions>(configuration.GetSection("Configuration"))
            .AddSingleton<IApiClient, ApiClient>()
            .AddSingleton<IAuthenticationTokenService, AuthenticationTokenService>()
            .AddTransient<IDataStoreService, DataStoreService>()
            .AddSingleton<IReportService, ReportService>()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });

        serviceCollection.AddHttpClient("RedditApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return serviceCollection.BuildServiceProvider();
    }
}