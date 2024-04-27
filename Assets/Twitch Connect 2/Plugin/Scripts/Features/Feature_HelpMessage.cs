using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TwitchConnect
{
    /// <summary>
    /// Internal class to reply help message in chat.
    /// </summary>
    public class Feature_HelpMessage : Feature
    {
        public Feature_HelpMessage() : base()
        {
            name = "Help message";

            Trigger_ChatCommand t_helpCommand = ScriptableObject.CreateInstance("Trigger_ChatCommand") as Trigger_ChatCommand;
            t_helpCommand.enabled = true;
            t_helpCommand.command = "help";

            Action_ReplyInChat_HelpMessage a_replyInChat = ScriptableObject.CreateInstance("Action_ReplyInChat_HelpMessage") as Action_ReplyInChat_HelpMessage;
            a_replyInChat.cooldown = 8f;
            a_replyInChat.duration = 0f;
            a_replyInChat.opened = true;

            triggers.Add(t_helpCommand);
            actions.Add(a_replyInChat);

            enabled = true;

            Initialize();
        }

    }
}