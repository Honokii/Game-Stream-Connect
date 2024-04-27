using System.Collections.Generic;

namespace TwitchConnect
{
    public class TokenRefreshResponse
    {
        public string access_token;
        public string refresh_token;
        public List<string> scope;
        public string token_type;
    }
}
