using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditPing.CLI.Services.Interfaces
{
    public interface IAuthenticationTokenService
    {
        public Task<string?> GetAccessToken();
        public Task SetAccessTokenAsync();
    }
}
