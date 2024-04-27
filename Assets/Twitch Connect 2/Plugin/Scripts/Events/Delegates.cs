namespace TwitchConnect
{
    public delegate void OnTriggerSuccesful(object sender, object payload, int userID, string userMessage);
    public delegate void OnNewViewer(int userID);
    public delegate void OnViewerLeft(int userID);
    public delegate void OnChannelRewardRedeem(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e);
}
