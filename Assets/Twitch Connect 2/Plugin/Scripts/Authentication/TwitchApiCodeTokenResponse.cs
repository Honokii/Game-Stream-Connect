using System;

namespace TwitchConnect
{
    /// <summary>
    /// Data object to parse API response for auth token into
    /// </summary>
    [Serializable]
    public class TwitchApiCodeTokenResponse
    {
        public string access_token;
        public int expires_in;
        public string refresh_token;
        public string[] scope;
        public string token_type;
    }
}