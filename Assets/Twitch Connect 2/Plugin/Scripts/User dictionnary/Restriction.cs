using System.Collections.Generic;

namespace TwitchConnect
{
    [System.Serializable]
    public struct Restriction
    {
        public bool enabled
        {
            get
            {
                return broadcaster || vips || moderators || staff || admins || global_mods; //|| subscribers);
            }
        }

        public byte count
        {
            get
            {
                byte result = 0;
                if (broadcaster) result++;
                if (vips) result++;
                if (moderators) result++;
                if (staff) result++;
                if (admins) result++;
                if (global_mods) result++;
                //if (subscribers) result++;

                return result;
            }
        }

        public bool broadcaster;
        public bool vips;
        public bool moderators;
        public bool staff;
        public bool admins;
        public bool global_mods;
        //public bool subscribers;

        public bool HasPrivilege(int userID)
        {
            string username = UserDictionnary.GetUserName(userID);

            return HasPrivilege(username);
        }

        public bool HasPrivilege(string u)
        {
            if (!enabled || TwitchManager.instance == null || TwitchManager.instance.twitchChannelInfos == null || TwitchManager.instance.twitchChannelInfos.chatters == null) return true;

            Chatters c = TwitchManager.instance.twitchChannelInfos.chatters;

            if (broadcaster && IsInList(u, c.broadcaster)) return true;
            if (vips && IsInList(u, c.vips)) return true;
            if (moderators && IsInList(u, c.moderators)) return true;
            if (staff && IsInList(u, c.staff)) return true;
            if (admins && IsInList(u, c.admins)) return true;
            if (global_mods && IsInList(u, c.global_mods)) return true;
            //if (subscribers && [SOMETHING SOMEHTING]) return true;

            return false;
        }

        private bool IsInList(string username, List<string> list)
        {
            if (!enabled) return true;

            foreach (string name in list)
            {
                if (username == name) return true;
            }

            return false;
        }
    }
}
