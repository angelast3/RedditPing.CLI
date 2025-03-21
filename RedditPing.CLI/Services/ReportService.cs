﻿using QuestPDF.Infrastructure;
using RedditPing.CLI.Models;
using RedditPing.CLI.Services.Interfaces;
using ScottPlot;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace RedditPing.CLI.Services
{
    public class ReportService : IReportService
    {

        public void GenerateReport(ReportInfo reportInfo)
        {
            string logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Reddit_Icon_FullColor.png");
            string lockupPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Reddit_Lockup_Bubble.png");

            // Generate the PDF report
            QuestPDF.Settings.License = LicenseType.Community;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().ShowOnce().Row(row =>
                    {
                        row.ConstantItem(120).PaddingRight(20).Image(lockupPath)
                            .FitWidth();
                        //row.ConstantItem(50).PaddingRight(20).Image(logoPath)
                        //    .FitWidth();
                        row.RelativeItem().AlignMiddle().Text($"Report  {DateTime.UtcNow.ToShortDateString()}")
                            .Style(TextStyle.Default.FontSize(17).Bold());
                    });


                    page.Content().Column(column =>
                    {
                         // Generate line charts for each subreddit
                        foreach (var subredditData in reportInfo.SubredditData)
                        {
                            // Subreddit title
                            column.Item().PaddingTop(20).Text(subredditData.Subreddit.DisplayName.ToString().ToUpper())
                                .Style(TextStyle.Default.FontSize(15).Bold().FontColor(QuestPDF.Infrastructure.Color.FromHex("#FF4500")));

                            var lineChartImagePath = GenerateLineChart(subredditData);
                            column.Item().Image(lineChartImagePath);
                            column.Item().PaddingBottom(20);

                            column.Item()
                                .BorderBottom((float)0.5)
                                .BorderColor(QuestPDF.Infrastructure.Color.FromHex("#FF4500"))
                                .PaddingBottom(5).Text("Trending Posts")
                                .Style(TextStyle.Default.FontSize(12).FontColor(QuestPDF.Infrastructure.Color.FromHex("#FF4500")));

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

                            // Display the top post first 
                            var topPost = allPosts.First();
                            ComposePostCard(column.Item(), topPost.Post, topPost.AverageScore, isTopPost: true);

                            foreach (var post in allPosts.Skip(1))
                            {
                                ComposePostCard(column.Item(), post.Post, post.AverageScore, isTopPost: false);

                            }
                        }
                    });
                });
            });

            document.GeneratePdf("DailyReport.pdf");
            Console.WriteLine($"Daily report generated successfully.");
        }

        // Generate a Line chart using Plot
        public string GenerateLineChart(SubredditData subredditData)
        {
            var subreddit = subredditData.Subreddit;
            var postsByTimestamp = subredditData.PostsByTimestamp;

            var plot = new Plot();
            plot.Title(subreddit.DisplayName);
            plot.XLabel("Time (hh:mm:ss)");
            plot.YLabel("Score");

            // Add a line color for each post
            var colors = new[] { ScottPlot.Colors.Red, ScottPlot.Colors.Green, ScottPlot.Colors.Blue, ScottPlot.Colors.Orange, ScottPlot.Colors.Purple,
                ScottPlot.Colors.Pink, ScottPlot.Colors.Black,ScottPlot.Colors.Brown, ScottPlot.Colors.Cyan, ScottPlot.Colors.Magenta, 
                ScottPlot.Colors.Yellow, ScottPlot.Colors.Lime, ScottPlot.Colors.Teal };
            var timestamps = postsByTimestamp.Keys.ToArray();

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
                    line.LegendText = post.Title.Length > 100 ? post.Title.Substring(0, 100) + "..." : post.Title;
                    line.Color = colors[addedPosts.Count % colors.Length];

                    // Mark the post as added to the legend
                    addedPosts.Add(post.Id);
                }
            }

            // Add a legend
            plot.ShowLegend(Edge.Bottom).Padding = new PixelPadding(0, 0, 0, 70);
            plot.Legend.OutlineWidth = (float)0.7;
            plot.Legend.OutlineColor = ScottPlot.Color.FromHex("#AFAEAE");
            plot.Legend.ShadowColor = ScottPlot.Color.FromHex("#FFFFFF");
            plot.Legend.FontSize = 12;

            // Customize X-axis to show time (hh:mm:ss) as labels
            plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                positions: timestamps.Select((t, index) => (double)index).ToArray(),
                labels: timestamps.Select(t => t.ToString()).ToArray()
            );

            plot.Axes.Bottom.TickLabelStyle.Rotation = 45; 
            plot.Axes.Bottom.TickLabelStyle.OffsetY = 20;
            plot.Axes.Bottom.Label.OffsetY = 25;
            plot.Axes.Bottom.Label.Padding = 10;

            // Save the line chart as an image
            var lineChartImagePath = $"{subreddit.DisplayName}_linechart.png";
            plot.SavePng(lineChartImagePath, 800, 700);
            return lineChartImagePath;
        }

        public void ComposePostCard(IContainer container, RedditPost post, double averageScore, bool isTopPost)
        {
            container
                .Border(0)
                .PaddingTop(12)
                .Column(column =>
                {
                    column.Item().Text(post.Title)
                        .Style(TextStyle.Default.FontSize(10).Bold());

                    column.Item().PaddingBottom(3).PaddingTop(3).Row(row =>
                    {
                        row.ConstantItem(300).Text(text =>
                        {
                            text.Span("Score ").FontSize(7).FontColor(QuestPDF.Infrastructure.Color.FromHex("000000"));
                            text.Span($"{averageScore:F0}   ").FontSize(10).FontColor(QuestPDF.Infrastructure.Color.FromHex("#FF4500"));
                            
                            text.Span("Comments ").FontSize(7).FontColor(QuestPDF.Infrastructure.Color.FromHex("000000"));
                            text.Span($"{post.NumComments}").FontSize(10).FontColor(QuestPDF.Infrastructure.Color.FromHex("#FF4500"));
                        });
                    });

                    if (!string.IsNullOrEmpty(post.LinkFlairText))
                    {
                        column.Item().PaddingBottom(2).PaddingLeft(8).Text(post.LinkFlairText)
                            .Style(TextStyle.Default.FontSize(8).FontColor(QuestPDF.Infrastructure.Color.FromHex("#464646")).Bold());
                    }

                    column.Item().Text(post.Url)
                        .Style(TextStyle.Default.FontSize(9).FontColor(QuestPDF.Infrastructure.Color.FromHex("#16161A")));
                });
        }
    }
}
