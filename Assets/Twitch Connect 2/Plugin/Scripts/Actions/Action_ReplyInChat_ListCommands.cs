using TwitchLib.Client.Events;

namespace TwitchConnect
{
    /// <summary>
    /// Internal class: Reply List of active commands to chat.
    /// </summary>
    [System.Serializable]
    public class Action_ReplyInChat_ListCommands : Action_ReplyInChat
    {
        public Action_ReplyInChat_ListCommands() : base()
        {

        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            if (payload is OnChatCommandReceivedArgs)
                UserDictionnary.AddToDictionnary(userID, (payload as OnChatCommandReceivedArgs).Command.ChatMessage.Username);

            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
                TwitchManager.instance.SendCommandList(UserDictionnary.GetUserName(userID));
        }
    }
}
