using System;
using TwitchLib.Client.Events;
using UnityEngine;

namespace GameStreamConnect {
    public class TwitchManagerEventBroadcaster : MonoBehaviour{
        
        [SerializeField] private OnTwitchCommandReceivedEvent onTwitchCommandReceivedEvent;
        private OnTwitchCommandReceivedEventArgs _onTwitchCommandReceivedEventArgs;

        private void Start() {
            _onTwitchCommandReceivedEventArgs = new OnTwitchCommandReceivedEventArgs();
        }

        public void HandleOnTwitchRewardRedeemed(int userID, string userMessage, object payload) {
            Debug.Log("Reward Redeemed: " + userMessage);
        }
        
        public void HandleOnTwitchCommandReceived(int userID, string userMessage, object payload) {
            Debug.Log("Command Received: " + userMessage);
            var command = (OnChatCommandReceivedArgs) payload;
            
            _onTwitchCommandReceivedEventArgs.UserID = userID;
            _onTwitchCommandReceivedEventArgs.UserMessage = userMessage;
            _onTwitchCommandReceivedEventArgs.Payload = command.Command;
            
            if (onTwitchCommandReceivedEvent != null)
                onTwitchCommandReceivedEvent.Raise(_onTwitchCommandReceivedEventArgs);
        }
    }
}