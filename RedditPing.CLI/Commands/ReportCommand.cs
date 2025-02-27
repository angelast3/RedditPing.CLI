using System.CommandLine;
using QuestPDF.Infrastructure;
using RedditPing.CLI.Models;
using ScottPlot;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace RedditPing.CLI.Commands
{
    public partial class CommandBuilder
    {
        public Command BuildReportCommand()
        {
            var postCommand = new Command("report", "Create Reddit report with trending posts");

            postCommand.AddCommand(DailyReportCommand());

            return postCommand;
        }
        public Command DailyReportCommand()
        {
            var subCommand = new Command("daily", "Generate a daily trending report");

            subCommand.SetHandler(h =>
            {
                try
                {
                    var data = _dataStoreService.LoadReportInfo();

                    if (data == null || data.SubredditData == null || !data.SubredditData.Any())
                    {
                        Console.WriteLine("No data available to generate the report.");
                        return;
                    }

                    GenerateDailyReport(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            return subCommand;
        }

        private void GenerateDailyReport(ReportInfo reportInfo)
        {
            // Generate the PDF report
            QuestPDF.Settings.License = LicenseType.Community; // Free license for open-source projects
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text($"Reddit Report - {reportInfo.Date}")
                        .Style(TextStyle.Default.FontSize(16).Bold());

                    page.Content().Column(column =>
                    {
                        column.Item().Text("Legend:")
                            .Style(TextStyle.Default.Bold());
                        column.Item().PaddingBottom(20);

                        // Generate line charts for each subreddit
                        foreach (var subredditData in reportInfo.SubredditData)
                        {
                            var lineChartImagePath = GenerateLineChart(subredditData);
                            column.Item().Image(lineChartImagePath);
                            column.Item().PaddingBottom(20);

                            column.Item().Text("Most Popular Posts (Average Score):")
                                .Style(TextStyle.Default.FontSize(12).Bold());

                            // Get the top 5 posts by average score
                            var allPosts = subredditData.PostsByTimestamp
                                    .SelectMany(t => t.Value)
                                    .GroupBy(p => p.Id) // Group posts by Id
                                    .Select(g => new
                                    {
                                        Post = g.First(), // Use the first post for metadata
                                        AverageScore = g.Average(p => p.Score)
                                    })
                                    .OrderByDescending(p => p.AverageScore)
                                    .ToList();

                            // Display the top post
                            var topPost = allPosts.First();
                            ComposePostCard(column.Item(), topPost.Post, topPost.AverageScore, isTopPost: true);

                            // Display the remaining posts in two columns
                            for (int i = 1; i < allPosts.Count; i += 2)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().Element(container => ComposePostCard(container, allPosts[i].Post, allPosts[i].AverageScore, isTopPost: false));
                                    if (i + 1 < allPosts.Count)
                                    {
                                        row.RelativeItem().Element(container => ComposePostCard(container, allPosts[i + 1].Post, allPosts[i + 1].AverageScore, isTopPost: false));
                                    }
                                });
                            }
                        }
                    });
                });
            });

            // Save the PDF
            document.GeneratePdf("DailyReport.pdf");
            Console.WriteLine($"Daily report generated successfully.");
        }

        // Generate a Line chart using Plot
        private static string GenerateLineChart(SubredditData subredditData)
        {
            var subreddit = subredditData.Subreddit;
            var postsByTimestamp = subredditData.PostsByTimestamp;

            var plot = new Plot();
            plot.Title(subreddit.DisplayName); // Subreddit title as chart title
            plot.XLabel("Time (hh:mm:ss)");
            plot.YLabel("Score");

            // Add a line for each post
            var colors = new[] { ScottPlot.Colors.Red, ScottPlot.Colors.Green, ScottPlot.Colors.Blue,
                ScottPlot.Colors.Orange, ScottPlot.Colors.Purple, ScottPlot.Colors.Pink, ScottPlot.Colors.Black };
            var timestamps = postsByTimestamp.Keys.OrderBy(t => t).ToArray(); // Sorted timestamps

            // Track posts that have already been added to the legend
            var addedPosts = new HashSet<string>();

            foreach (var timestamp in timestamps)
            {
                var posts = postsByTimestamp[timestamp];
                foreach (var post in posts)
                {
                    // Skip if the post has already been added to the legend
                    if (addedPosts.Contains(post.Id))
                        continue;

                    var scores = new double[timestamps.Length];
                    for (int i = 0; i < timestamps.Length; i++)
                    {
                        var currentTimestamp = timestamps[i];
                        scores[i] = postsByTimestamp[currentTimestamp]
                            .FirstOrDefault(p => p.Id == post.Id)?.Score ?? double.NaN;
                    }

                    var line = plot.Add.Scatter(timestamps.Select((t, index) => (double)index).ToArray(), scores);
                    line.LegendText = post.Title;
                    line.Color = colors[addedPosts.Count % colors.Length];

                    // Mark the post as added to the legend
                    addedPosts.Add(post.Id);
                }
            }

            // Add a legend
            plot.ShowLegend(Edge.Bottom);
            plot.Legend.FontSize = 12;

            // Customize X-axis to show time (hh:mm:ss) as labels
            plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                positions: timestamps.Select((t, index) => (double)index).ToArray(),
                labels: timestamps
            );

            // Save the line chart as an image
            var lineChartImagePath = $"{subreddit.DisplayName}_linechart.png";
            plot.SavePng(lineChartImagePath, 800, 600);
            return lineChartImagePath;
        }

        private static void ComposePostCard(IContainer container, RedditPost post, double averageScore, bool isTopPost)
        {
            container
                .Border(0)
                .Padding(20)
                .Column(column =>
                {
                    column.Item().Text(post.Title)
                        .Style(TextStyle.Default.FontSize(12).Bold());

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Scores: {averageScore:F0}")
                            .Style(TextStyle.Default.FontSize(11).FontColor(QuestPDF.Infrastructure.Color.FromHex("#C9420C")));
                        row.RelativeItem().Text($"Comments: {post.NumComments}")
                            .Style(TextStyle.Default.FontSize(11).FontColor(QuestPDF.Infrastructure.Color.FromHex("#C9420C")));
                    });

                    // Link flair text (if available)
                    if (!string.IsNullOrEmpty(post.LinkFlairText))
                    {
                        column.Item().PaddingBottom(3).Text(post.LinkFlairText)
                            .Style(TextStyle.Default.FontSize(9).FontColor(QuestPDF.Infrastructure.Color.FromHex("#16161A")));
                    }

                    column.Item().Text(post.Url)
                        .Style(TextStyle.Default.FontSize(9).FontColor(QuestPDF.Infrastructure.Color.FromHex("#16161A")));
                });
        }
    }
}
