namespace TwitchConnect
{
    /// <summary>
    /// Internal class: Reply Help message to chat.
    /// </summary>
    [System.Serializable]
    public class Action_ReplyInChat_HelpMessage : Action_ReplyInChat
    {
        public Action_ReplyInChat_HelpMessage() : base()
        {

        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            UserDictionnary.AddToDictionnary(userID, userMessage);

            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
                TwitchManager.instance.SendHelpMessage(UserDictionnary.GetUserName(userID));
        }
    }
}