using System.Collections.Generic;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Internal class to reply command list in chat.
    /// </summary>
    public class Feature_CommandList : Feature
    {
        public Feature_CommandList() : base()
        {
            name = "Command list";

            Trigger_ChatCommand t_helpCommand = ScriptableObject.CreateInstance("Trigger_ChatCommand") as Trigger_ChatCommand;
            t_helpCommand.enabled = true;
            t_helpCommand.command = "commands";

            Action_ReplyInChat_ListCommands a_replyInChat = ScriptableObject.CreateInstance("Action_ReplyInChat_ListCommands") as Action_ReplyInChat_ListCommands;
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