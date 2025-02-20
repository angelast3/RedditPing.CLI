using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RedditPing.CLI.Commands;
using RedditPing.CLI.Configuration.Implementation;
using RedditPing.CLI.Configuration.Model;
using RedditPing.CLI.Services;
using RedditPing.CLI.Services.Interfaces;

namespace RedditPing.CLI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).
            AddEnvironmentVariables()
            .Build();

            //services.Configure<ConfigurationOptions>(configuration.GetSection("Configuration"));
            //services.AddSingleton<IConfigureOptions<ConfigurationOptions>, ConfigureOptions>();

            services.AddSingleton<IApiClient, ApiClient>();


            return services;
        }
    }
}
