using System.Collections.Generic;

namespace TwitchConnect
{
    public class UserValidateResponse
    {
        public string client_id;
        public string login;
        public List<string> scopes;
        public string user_id;
        public int expires_in;
    }
}
