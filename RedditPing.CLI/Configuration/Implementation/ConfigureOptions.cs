using Microsoft.Extensions.Options;
using RedditPing.CLI.Configuration.Model;

namespace RedditPing.CLI.Configuration.Implementation
{
    public class ConfigureOptions : IConfigureOptions<ConfigurationOptions>
    {
        void IConfigureOptions<ConfigurationOptions>.Configure(ConfigurationOptions options) { }
    }
}
