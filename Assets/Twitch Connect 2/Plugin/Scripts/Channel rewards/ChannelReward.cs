using UnityEngine;

namespace TwitchConnect
{
    [System.Serializable]
    //[CreateAssetMenu(fileName ="New channel reward", menuName ="TwitchConnect/Channel reward", order = 809)]
    public class ChannelReward : ScriptableObject
    {
        [Header("General")]
        public string title;
        public string prompt;
        public int cost;
        public bool isEnabled;
        [Header("Images")]
        public string backgroundColor;
        public string imageURLx1;
        public string imageURLx2;
        public string imageURLx3;
        [Header("Settings")]
        public bool isUserInputRequired;
        public bool isMaxPerStreamEnabled;
        public int maxPerStream;
        public bool isMaxPerUserPerStreamEnabled;
        public int maxPerUserPerStream;
        public bool isGlobalCooldownEnabled;
        public int globalCooldownSeconds;
        public bool shouldRedemptionsSkipRequestQueue;

        // set only when the reward is created or is known
        private int knownId;
        private int broadcasterId;
    }
}

