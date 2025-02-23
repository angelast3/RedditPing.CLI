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
        public Command BuildPostCommand()
        {
            var postCommand = new Command("post", "Manage Reddit posts");

            postCommand.AddCommand(ListPostsCommand());
            postCommand.AddCommand(ListPopularPostsCommand());

            return postCommand;
        }

        private Command ListPostsCommand()
        {
            var subCommand = new Command("list", "List posts based on a subreddit");
            var topicOption = new Option<string>(aliases: ["-sr", "-subreddit"], "Topic to filter posts by");
            var typeOption = new Option<PostType>(aliases: ["-st", "-search-type"], () => 0, "Search posts by type. Hot - 0, New - 1, Top - 2, Rising - 3");
            var limitOption = new Option<int>(aliases: ["-l", "-limit"], () => 10, "Number of posts to fetch");

            subCommand.AddOption(topicOption);
            subCommand.AddOption(typeOption);
            subCommand.AddOption(limitOption);

            subCommand.SetHandler(async h =>
            {
                try
                {
                    var topic = h.ParseResult.GetValueForOption(topicOption);
                    var type = h.ParseResult.GetValueForOption(typeOption);
                    var limit = h.ParseResult.GetValueForOption(limitOption);

                    _logger.LogInformation("Fetching posts for topic: {Topic}, type: {Type}, limit: {Limit}", topic, type, limit); // Log info

                    var url = AppConstants.PostsBaseUrl + $"r/{topic}/{type.ToString().ToLower()}.json?limit={limit}";
                    var jsonResponse = await _apiClient.GetAsync(url);

                    _logger.LogInformation("Received response from API");

                    var redditResponse = JsonSerializer.Deserialize<List<RedditResponse<RedditPost>>>(jsonResponse)
                        ?? new List<RedditResponse<RedditPost>>();

                    foreach (var post in redditResponse)
                    {
                        if (!post.data.over_18)
                            Console.WriteLine(JsonSerializer.Serialize(post, new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            }));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching posts"); // Log error
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            return subCommand;
        }

        private Command ListPopularPostsCommand()
        {
            var subCommand = new Command("add", "Retrive posts from the defined subreddits and save them for report generation.");
            var typeOption = new Option<PostType>(aliases: ["-st", "-search-type"], () => 0, "Search posts by type. Hot - 0, New - 1, Top - 2, Rising - 3");
            var limitOption = new Option<int>(aliases: ["-l", "-limit"], () => 5, "Number of posts to fetch");

            subCommand.AddOption(typeOption);
            subCommand.AddOption(limitOption);

            subCommand.SetHandler(async h =>
            {
                try
                {
                    var type = h.ParseResult.GetValueForOption(typeOption);
                    var limit = h.ParseResult.GetValueForOption(limitOption);

                    _logger.LogInformation("Fetching popular posts, type: {Type}, limit: {Limit}", type, limit);

                    var reportnfo = _dataStoreService.LoadReportInfo();

                    if (reportnfo != null && reportnfo.SubredditData != null && reportnfo.SubredditData.Count > 0)
                    {
                        var subredditPosts = new List<SubredditReport>();

                        foreach (var subredditData in reportnfo.SubredditData)
                        {
                            var url = AppConstants.PostsBaseUrl + $"{subredditData.Subreddit.display_name_prefixed}/{type.ToString().ToLower()}.json?limit={limit}";
                            var jsonResponse = await _apiClient.GetAsync(url);

                            var redditResponse = JsonSerializer.Deserialize<List<RedditResponse<RedditPost>>>(jsonResponse)
                                ?? new List<RedditResponse<RedditPost>>();

                            var posts = new List<RedditPost>();
                            foreach (var item in redditResponse)
                            {
                                if (!item.data.over_18)
                                    posts.Add(item.data);
                            }

                            subredditPosts.Add(new SubredditReport()
                            {
                                Id = subredditData.Subreddit.id,
                                Posts = posts
                            });

                        }
                        _dataStoreService.UpdatePosts(subredditPosts);

                        _logger.LogInformation("Successfully retrieved posts.");
                        Console.WriteLine($"Successfully retrieved posts!");
                    }
                    else
                    {
                        _logger.LogWarning("No subreddit data found for report info.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching popular posts");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            return subCommand;
        }
    }
    
}
