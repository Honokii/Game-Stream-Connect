using System.Collections.Generic;

namespace TwitchConnect
{
    /// <summary>
    /// Static dictionnary that associates userIDs (int) to last known usernames.
    /// </summary>
    public static class UserDictionnary
    {
        private static Dictionary<int, string> dictionnary = new Dictionary<int, string>();

        private static string result;
        private static int id;

        /// <summary>
        /// Returns the username of a viewer identified by its userID.
        /// <para>For performance reasons, most workload on viewers is done using userIDs.</para>
        /// <para>Keep in mind that viewers can change username whenever they want. This dictionnary is updated live when users interacts with the chat.</para>
        /// <para>Use this method whenever you need to display viewers name on-screen.</para>
        /// </summary>
        /// <param name="_id">UserID.</param>
        /// <returns>Returns the last known username as string.</returns>
        public static string GetUserName(int _id)
        {
            if (dictionnary.TryGetValue(_id, out result)) return result;

            return "- user " + _id.ToString() + " not found -";
        }

        public static void AddToDictionnary(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            id = int.Parse(e.ChatMessage.UserId);

            AddToDictionnary(id, e.ChatMessage.Username);
        }

        public static bool ContainsUserID(int userID)
        {
            return dictionnary.ContainsKey(userID);
        }

        public static void AddToDictionnary(int userID, string userName)
        {
            if (!dictionnary.ContainsKey(userID))
                dictionnary.Add(userID, userName);
            else
                dictionnary[userID] = userName;
        }

        public static void Initialize()
        {
            dictionnary = new Dictionary<int, string>();
            TwitchManager.instance.client.OnMessageReceived += AddToDictionnary;
        }

        public static void ReInitialize()
        {
            TwitchManager.instance.client.OnMessageReceived += AddToDictionnary;
        }

        public static void Clear()
        {
            if (dictionnary!= null) dictionnary.Clear();
        }

        public static bool UsernameExists(string username)
        {
            return dictionnary.ContainsValue(username);
        }

        public static int GetUserIDFromUsername(string username)
        {
            foreach (KeyValuePair<int, string> kvp in dictionnary)
            {
                if (kvp.Value == username)
                    return kvp.Key;
            }

            return -1;
        }
    }
}
