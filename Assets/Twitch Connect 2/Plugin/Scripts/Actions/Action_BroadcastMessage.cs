using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Broadcast the message to scene receivers
    /// </summary>
    [System.Serializable]
    public class Action_BroadcastMessage : Action
    {
        [Tooltip("Custom broadcast name by which receivers will be triggered to display a new message.")]
        /// <summary>
        /// Name by which receivers will be triggered to display a new message.
        /// </summary>
        public string channel;

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
                TwitchManager.messageBroadcaster.Publish(channel, UserDictionnary.GetUserName(userID), userMessage);
        }
#if UNITY_EDITOR
        public override string InspectorName()
        {
            return "Broadcast to channel: " + channel;
        }

        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();
                EditorGUI.indentLevel++;
                ShowProperty(new SerializedObject(this), "channel");
                EditorGUI.indentLevel--;
            }
        }
#endif
    }
}
