using System.Text.Json.Serialization;

namespace RedditPing.CLI.Models;
#pragma warning disable CS8618
public class SubReddit
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("display_name_prefixed")]
    public string DisplayNamePrefixed { get; set; }

    [JsonPropertyName("over18")]
    public bool Over18 { get; set; }
}


