using UnityEngine;

namespace GameStreamConnect.Community {
    public class TwitchCommunity : MonoBehaviour {
        
        public void HandleOnTwitchRewardRedeemed(int userID, string userMessage, object payload) {
            Debug.Log("Reward Redeemed: " + userMessage);
        }
        
        public void HandleOnTwitchCommandReceived(int userID, string userMessage, object payload) {
            Debug.Log("Command Received: " + userMessage);
        }
    }
}