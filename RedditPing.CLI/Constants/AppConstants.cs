namespace RedditPing.CLI.Constants
{
    public static class AppConstants
    {
        // Config
        public const string TokenUrl = "https://www.reddit.com/api/v1/access_token";
        public const string CLIName = "rpcli";

        // Urls
        public const string PostsBaseUrl = "https://oauth.reddit.com/";
        public const string SubRedditsBaseUrl = "https://oauth.reddit.com/subreddits/";

        //Date formats
        public const string DateFormat = "dd-MM-yyyy";
        public const string DateTimeFormat = "HH:mm:ss";

        public const string PostApiPath = "/post/";
    }
}
