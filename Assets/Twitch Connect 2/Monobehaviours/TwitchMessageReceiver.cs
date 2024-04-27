using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using TwitchLib.Client.Models;
using System;

namespace TwitchConnect
{
    /// <summary>
    /// Component to format and display messages received through a channel created by a Broadcast action.
    /// </summary>
    public class TwitchMessageReceiver : MonoBehaviour
    {
        #region Public variables

        #region General settings
        [Header("General Settings")]

        [Tooltip("Display new messages ?")]
        /// <summary>
        /// Display new messages ?
        /// </summary>
        public bool update = true;

        [Tooltip("Display all log messages in console?")]
        /// <summary>
        /// Display all log messages in console.
        /// </summary>
        public bool debug = false;

        [Tooltip("Channel name: messages will be received when the channel-corresponding command is sent from a viewer.")]
        /// <summary>
        /// Channel name: messages will be received when the channel-corresponding command is sent from a viewer.
        /// </summary>
        public string channelName;


        [Tooltip("Publicly exposed last message received.")]
        /// <summary>
        /// Publicly exposed last message received.
        /// </summary>
        public TwitchMessage lastReceivedMessage;
        #endregion

        #region Format message

        [Header("Auto-format message")]

        [Tooltip("Create auto-formatted messages using parameters.")]
        /// <summary>
        /// Format messages automatically ?
        /// </summary>
        public bool autoFormatMessages = true;

        [TextArea]
        [Tooltip("Auto-format template.")]
        /// <summary>
        /// Auto-format template.
        /// </summary>
        public string customMessageFormat = "##TIME## - ##SENDER## - ##MESSAGE##";

        [Tooltip("Adds a carriage return character at end of message.")]
        /// <summary>
        /// Adds a carriage return character at end of message.
        /// </summary>
        public bool endCharCr = false;

        [Tooltip("Adds a new line character at end of message.")]
        /// <summary>
        /// Adds a new line character at end of message.
        /// </summary>
        public bool endCharNl = true;

        [Tooltip("Adds a custom character (or even string) at end of message.")]
        /// <summary>
        /// Adds a custom character or string at end of message.
        /// </summary>
        public string customEndOfMessage = "";

        [Tooltip("Publicly exposed last message formatted.")]
        /// <summary>
        /// Publicly exposed last message received.
        /// </summary>
        public string lastFormattedMessage;

        // X last received messages
        public Queue<TwitchMessage> tmQueue = new Queue<TwitchMessage>();

        #endregion

        #region Update target

        [Header("Auto-update target text")]

        [Tooltip("Automatically update target text value ?")]
        /// <summary>
        /// Automatically update target text value ?
        /// </summary>
        public bool targetUpdate = true;

        [Tooltip("Target Text component is on the same gameObject ? If so, tick this option to automatically target it as this script's target.")]
        /// <summary>
        /// Target Text component is on the same gameObject ? If so, tick this option to automatically target it as this script's target.
        /// </summary>
        public bool textTargetOnSameGameObject = false;

        [Tooltip("The target to udpate with messages received.")]
        /// <summary>
        /// The target to udpate with messages received.
        /// </summary>
        public Text targetText;
        // public TextMeshPro targetTMP;
        // public Object targetObject;

        [HideInInspector]
        public string textContent;

        #endregion

        #endregion

        #region Private variables

        private TwitchMessageBroadcaster broadcaster;

        [Tooltip("Has this object registered into TwitchConnect object?")]
        /// <summary>
        /// Has this object registered into TwitchConnect object?
        /// </summary>
        private bool foundTwitchConnect;

        [SerializeField]
        [Tooltip("Maximum number of lastly received Viewers message to keep in memory. 0 = unlimited, in which case don't forget to empty it manually from times to times to avoid memory overload")]
        private int savedMessageLimit = 30;

        private int queueCount;

        private bool old_textTargetOnSameGameObject;

        #endregion

        #region Monobehaviour Callbacks

        private void Start()
        {
            SubscribeToTwitchConnectInstance();

            old_textTargetOnSameGameObject = textTargetOnSameGameObject;

            if (textTargetOnSameGameObject)
            {
                if (targetText != null)
                {
                    Debug.LogWarning("[TwitchMessageReceiver - " + channelName + " - Auto-update text feature] An object has been provided as target text object, but the 'Text Target On Same Game Object' boolean is active. The text found on the game object will be updated.");
                }

                targetText = GetComponent<Text>();

                if (debug && targetText)
                {
                    Debug.Log("[TwitchMessageReceiver - " + channelName + "] Succesfully retrieved target text.");
                }

                textContent = targetText.text;
            }
        }

        private void Update()
        {
            if (textTargetOnSameGameObject != old_textTargetOnSameGameObject) // someone clicked or modified this property
            {
                if (textTargetOnSameGameObject == true)
                {
                    if (targetText != null)
                    {
                        Debug.LogWarning("[TwitchMessageReceiver - " + channelName + "] The previous text target has been removed because the 'textTargetOnSameGameObject' property has changed from false to true.");
                    }

                    targetText = GetComponent<Text>();

                    if (targetText == null)
                    {
                        Debug.LogWarning("[TwitchMessageReceiver - " + channelName + "] Unable to retrieve Text component on this gameObject.");
                    }
                }
                else
                {
                    // remove the target only if it was the text on the same gameObject
                    Text textOnMe = GetComponent<Text>();
                    if (textOnMe != null && textOnMe == targetText)
                    {
                        targetText = null;
                    }
                }
            }

            old_textTargetOnSameGameObject = textTargetOnSameGameObject;

            targetText.text = textContent;
        }

        private void OnDestroy()
        {
            UnsuscribeFromTwitchConnectInstance();
        }

        private void OnDisable()
        {
            UnsuscribeFromTwitchConnectInstance();
        }

        #endregion

        #region Public methods

        public void NewMessage(TwitchMessage tm)
        {

            // Save message in queue
            if (queueCount < savedMessageLimit)
            {
                tmQueue.Enqueue(tm);
                queueCount += 1;
            }
            else
            {
                tmQueue.Enqueue(tm);
                tmQueue.Dequeue();
            }

            lastReceivedMessage = tm;

            if (debug) Debug.Log("[TwitchMessageReceiver - " + channelName + "] Succesfully received message from viewer " + tm.sender);

            // Format text

            if (autoFormatMessages)
            {
                lastFormattedMessage = FormattedMessage(tm);
            }
            else
            {
                lastFormattedMessage = tm.message;
            }

            // Update text
            if (targetUpdate && targetText != null)
            {
                textContent = lastFormattedMessage + textContent;
            }

        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns the formatted message to display with given twitch message to a (return
        /// </summary>
        /// <param name="tm"></param>
        /// <returns></returns>
        private string FormattedMessage(TwitchMessage tm)
        {
            string result = customMessageFormat;

            result = result.Replace("##SENDER##", tm.sender);
            result = result.Replace("##MESSAGE##", tm.message);
            result = result.Replace("##TIME##", tm.receivedTime.ToString("T"));

            if (endCharCr)
            {
                result += '\r';
            }

            if (endCharNl)
            {
                result += '\n';
            }

            result += customEndOfMessage;

            if (debug)
            {
                Debug.Log("[TwitchMessageReceiver - " + channelName + "] Formatted last received message to : " + result);
            }

            return result;
        }

        /// <summary>
        /// Register from TwitchConnect's list of TwitchMessageReceivers by calling RegisterReceiver in TwitchConnect class.
        /// </summary>
        public virtual void SubscribeToTwitchConnectInstance()
        {
            if (TwitchManager.messageBroadcaster == null)
            {
                if (debug)
                {
                    Debug.LogWarning("[TwitchMessageReceiver - " + channelName + "] Unable to register to TwitchConnect: instance not found.");
                }

                return;
            }

            broadcaster = TwitchManager.messageBroadcaster;

            foundTwitchConnect |= broadcaster.RegisterReceiver(this);

            if (foundTwitchConnect && debug) Debug.Log("[TwitchMessageReceiver - " + channelName + "] Succesfully subscribed to TwitchConnect.");
        }

        /// <summary>
        /// Unsubscribe from TwitchConnect's list of TwitchMessageReceivers by calling RemoveReceiver in TwitchConnect class.
        /// </summary>
        public virtual void UnsuscribeFromTwitchConnectInstance()
        {
            foundTwitchConnect = false;

            if (broadcaster == null)
            {
              /*if (debug)
                {
                    Debug.LogWarning("[TwitchMessageReceiver - " + channelName + "] Unable to unregister from TwitchConnect: instance not found.");
                }*/

                // actually not an issue
                return;
            }

            if (broadcaster.RemoveReceiver(this) && debug) Debug.Log("[TwitchMessageReceiver - " + channelName + "] Succesfully unsubscribed from Broadcaster.");
        }

        #endregion
    }
}