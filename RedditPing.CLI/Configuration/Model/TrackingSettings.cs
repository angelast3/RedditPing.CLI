using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditPing.CLI.Configuration.Model
{
    public class TrackingSettings
    {
        public List<string>? Subreddits { get; set; }
        public List<string>? Keywords { get; set; }
    }
}
