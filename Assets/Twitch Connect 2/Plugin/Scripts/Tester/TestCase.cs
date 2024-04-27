using System.Collections.Generic;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Scriptable Object that holds test cases and other IRC messages to simplify testing process.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName ="new test case", menuName ="TwitchConnect/Test case", order = 808)]
    public class TestCase : ScriptableObject
    {
        public enum ERecordTarget
        {
            chatMessages,
            chatCommands,
            chatCommandsWithArgument,
            bitsDonations,
            newSuscribers
        }

        public string[] chatMessages = new string[0];
        public string[] chatCommands = new string[0];
        public string[] chatCommandsWithArgument = new string[0];
        public string[] bitsDonations = new string[0];
        public string[] newSuscribers = new string[0];
        public string[] otherMessage = new string[0];

        public string[] channelRewards = new string[0];

        [HideInInspector]
        public bool inspectorFoldout;

        [HideInInspector]
        public ERecordTarget recordTarget;
        [HideInInspector]
        public bool recordReady;
        [HideInInspector]
        public int recordIndex;


        public Queue<string> lastMessages = new Queue<string>();
        private const int maxMessages = 10;

        public bool SendTestMessage(ref TwitchManager tm, string[] messageSet, int index)
        {
            if (tm != null && tm.client != null && tm.client.IsInitialized && messageSet[index] != "")
            {
                tm.client.OnReadLineTest(messageSet[index]);
                return true;
            }
            return false;
        }

        public bool SendTestPubSub(ref TwitchManager tm, string[] messageSet, int index)
        {
            if (TwitchManager.pubSub != null && messageSet[index] != "")
            {
                //TwitchManager.pubSub.TestMessageParser(JsonUtility.ToJson(messageSet[index]));
                TwitchManager.pubSub.TestMessageParser(messageSet[index]);
                return true;
            }
            return false;
        }

        public void RecordMessage(object sender, TwitchLib.Client.Events.OnSendReceiveDataArgs e)
        {
            if (e.Data.StartsWith("@"))
                lastMessages.Enqueue(e.Data);

            while (lastMessages.Count > maxMessages)
            {
                lastMessages.Dequeue();
            }
        }

        public void Initialize(TwitchManager tm)
        {
            tm.client.OnSendReceiveData += RecordMessage;
        }

        public void DeInitialize(TwitchManager tm)
        {
            tm.client.OnSendReceiveData -= RecordMessage;
        }
    }
}