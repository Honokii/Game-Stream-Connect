using UnityEngine;
using System.Collections;

namespace TwitchConnect
{
    /// <summary>
    /// Component that can receive all chat messages (without needing a broadcast action).
    /// </summary>
    public class TwitchChatReceiver : TwitchMessageReceiver
    {
        public override void SubscribeToTwitchConnectInstance()
        {
            channelName = "TwitchChat";

            if (TwitchManager.instance == null)
            {
                Debug.LogWarning("[Twitch Chat Receiver on " + gameObject.name + " game object] - No Twitch Manager found in scene. Please make sure one is present.");
                return;
            }

            TwitchManager.instance.EnableTwitchChatFeature();

            base.SubscribeToTwitchConnectInstance();
        }
    }
}