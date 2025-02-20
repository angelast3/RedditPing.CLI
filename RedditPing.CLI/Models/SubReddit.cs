using System.Text.Json.Serialization;

namespace RedditPing.CLI.Models;
#pragma warning disable CS8618
public class SubReddit
{
    public string id { get; set; }
    public string name { get; set; }
    public string title { get; set; }
    public string display_name { get; set; }
    public string display_name_prefixed { get; set; }
    [JsonIgnore]
    public bool over18 { get; set; }
}

