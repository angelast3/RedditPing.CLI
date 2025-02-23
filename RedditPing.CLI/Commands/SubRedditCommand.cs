using System.CommandLine;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RedditPing.CLI.Constants;
using RedditPing.CLI.Enums;
using RedditPing.CLI.Models;

namespace RedditPing.CLI.Commands
{
    public partial class CommandBuilder
    {
        public Command BuildSubredditCommand()
        {
            var postCommand = new Command("subreddit", "Search subreddits");

            postCommand.AddCommand(ListSubredditsCommand());

            return postCommand;
        }

        private Command ListSubredditsCommand()
        {
            var subCommand = new Command("list", "Get all subreddits based on a type");
            var typeOption = new Option<SubRedditType>(aliases: ["-st", "-search-type"], () => 0, "Search subreddits by type. Popular - 0, New - 1, Default - 2");
            var limitOption = new Option<int>(aliases: ["-l", "-limit"], () => 3, "Number of posts to fetch");

            subCommand.AddOption(typeOption);
            subCommand.AddOption(limitOption);

            subCommand.SetHandler(async h =>
            {
                try
                {
                    var type = h.ParseResult.GetValueForOption(typeOption);
                    var limit = h.ParseResult.GetValueForOption(limitOption);

                    _logger.LogInformation("Fetching subreddits, type: {Type}, limit: {Limit}", type, limit);

                    var url = AppConstants.SubRedditsBaseUrl + $"{type.ToString().ToLower()}.json?limit={limit}";

                    var jsonResponse = await _apiClient.GetAsync(url);
                    var redditResponse = JsonSerializer.Deserialize<List<RedditResponse<SubReddit>>>(jsonResponse)
                        ?? [];

                    foreach (var subreddit in redditResponse)
                    {
                        if (!subreddit.data.over18)
                            Console.WriteLine(JsonSerializer.Serialize(subreddit, new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            }));
                    }

                    var subredditsList = redditResponse.Select(x => new SubReddit
                    {
                        id = x.data.id,
                        name = x.data.name,
                        title = x.data.title,
                        display_name = x.data.display_name,
                        display_name_prefixed = x.data.display_name_prefixed
                    }).ToList();

                    _dataStoreService.UpdateSubreddits(subredditsList);

                    _logger.LogInformation("Successfully fetched and updated subreddits.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching subreddits"); // Log error
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            return subCommand;
        }
    }

}
