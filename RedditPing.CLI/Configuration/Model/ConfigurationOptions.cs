namespace RedditPing.CLI.Configuration.Model
{
    public class ConfigurationOptions
    {
        public string? FrequencyInMinutes { get; set; }
        public ClientConfig? ClientConfig { get; set; }

        public TrackingSettings? TrackingSettings { get; set; }
    }
}
