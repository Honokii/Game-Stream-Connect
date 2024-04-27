namespace TwitchConnect
{
    /// <summary>
    /// Internal class used to broadcast any chat message to all TwitchChatReceiver in scene.
    /// </summary>
    public class Trigger_ChatMessage_AllMessages : Trigger_ChatMessage
    {
        public Trigger_ChatMessage_AllMessages() : base()
        {
            enabled = true;
        }

        public override void OnTrigger(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            chatText = e.ChatMessage.Message; // adjust message each time
            base.OnTrigger(this, e);
        }
#if UNITY_EDITOR
        public override void DrawInspector()
        {

        }

        public override string InspectorName()
        {
            return "Any chat messages";
        }
#endif
    }
}