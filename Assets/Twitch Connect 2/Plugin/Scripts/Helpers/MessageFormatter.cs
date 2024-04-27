namespace TwitchConnect
{
    /// <summary>
    /// Helper that formats messages on given template.
    /// </summary>
    public static class MessageFormatter
    {

        public const string senderTag = "_sender_";
        public const string originalMessageTag = "_message_";
        public const string timeTag = "_time_";

        public static string FormatMessage(string templateUsingTags, System.DateTime time, string viewersName = "", string viewersMessage = "")
        {
            string result = templateUsingTags;

            result = result.Replace(senderTag, viewersName);
            result = result.Replace(originalMessageTag, viewersMessage);
            result = result.Replace(timeTag, time.ToString("T"));

            return result;
        }

        public static string FormatMessage(string templateUsingTags, System.DateTime time, int viewersID = 0, string viewersMessage = "")
        {
            return FormatMessage(templateUsingTags, time, UserDictionnary.GetUserName(viewersID), viewersMessage);
        }
    }
}