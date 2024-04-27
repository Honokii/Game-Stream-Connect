namespace TwitchConnect
{
    /// <summary>           
    /// Simple struct for TwitchMessage (taken from Twitch Connect v1).           
    /// </summary>
    [System.Serializable]
    public class TwitchMessage
    {
        public string sender;
        public string message;
        public System.DateTime receivedTime;
        /// <summary>
        /// Create a new Twitch Message with given parameters. Received time is the construction time. Change manually afterhands if needed.
        /// </summary>
        /// <param name="senderViewer"></param>
        /// <param name="messageViewer"></param>
        public TwitchMessage(string senderViewer, string messageViewer)
        {
            // Usual constructor
            sender = senderViewer;
            message = messageViewer;
            receivedTime = System.DateTime.Now;
        }
        /// <summary>
        /// Create a new twitchMessage identical to the twitch message it is passed (including time of creation. Change manually afterhands if needed.
        /// </summary>
        /// <param name="tm"></param>
        public TwitchMessage(TwitchMessage tm)
        {
            // Copy constructor
            sender = tm.sender;
            message = tm.message;
            receivedTime = tm.receivedTime;
        }
    }
}