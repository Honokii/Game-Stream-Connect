using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TwitchConnect
{
    /// <summary>
    /// Internal feature class for broadcasting chat messages to TwitchChatReceiver.
    /// </summary>
    public class Feature_Chat : Feature
    {
        public Feature_Chat() : base()
        {
            name = "Show chat messages";

            Trigger_ChatMessage_AllMessages t_allChatMessages = ScriptableObject.CreateInstance("Trigger_ChatMessage_AllMessages") as Trigger_ChatMessage_AllMessages;
            t_allChatMessages.enabled = true;

            Action_BroadcastMessage a_broadcastAllMessages = ScriptableObject.CreateInstance("Action_BroadcastMessage") as Action_BroadcastMessage;
            a_broadcastAllMessages.cooldown = 0f;
            a_broadcastAllMessages.duration = 0f;
            a_broadcastAllMessages.opened = true;
            a_broadcastAllMessages.channel = "TwitchChat";


            triggers.Add(t_allChatMessages);
            actions.Add(a_broadcastAllMessages);

            enabled = true;

            Initialize();
        }

    }
}