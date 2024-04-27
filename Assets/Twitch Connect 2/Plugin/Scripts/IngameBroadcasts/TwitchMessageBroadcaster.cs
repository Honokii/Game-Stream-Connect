using System.Collections.Generic;

namespace TwitchConnect
{
    /// <summary>
    /// Internal class to help broadcasting messages to in-game channels defined by TwitchMessageReceiver.
    /// </summary>
    public class TwitchMessageBroadcaster
    {
        private List<TwitchMessageReceiver> receivers = new List<TwitchMessageReceiver>();

        public bool RegisterReceiver(TwitchMessageReceiver receiver)
        {
            if (receivers == null)
                receivers = new List<TwitchMessageReceiver>();

            if (!receivers.Contains(receiver))
            {
                receivers.Add(receiver);
                return true;
            }

            return false;
        }

        public bool RemoveReceiver(TwitchMessageReceiver receiver)
        {
            if (receivers == null)
                return false;

            if (receivers.Contains(receiver))
            {
                receivers.Remove(receiver);
                return true;
            }

            return false;
        }

        public void Publish(string channel, string sender, string message)
        {
            foreach (TwitchMessageReceiver tr in receivers)
            {
                if (tr.channelName == channel)
                    tr.NewMessage(new TwitchMessage(sender, message));
            }
        }
    }
}