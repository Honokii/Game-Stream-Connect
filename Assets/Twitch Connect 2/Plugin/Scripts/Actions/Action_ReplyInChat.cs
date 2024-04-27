using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Post a custom reply in chat.
    /// </summary>
    [System.Serializable]
    public class Action_ReplyInChat : Action
    {
        private const string defaultTemplate = MessageFormatter.senderTag + " just cheered you at " + MessageFormatter.timeTag;
        private const string TurkenbergsMessage = "Hi, i am 0x6E71e1f94d56E289d55A5955E7dC70f1AE3F2254";
        private GUIStyle labelWordWrap = new GUIStyle();

        [Tooltip("Message template used to format incoming chat message into a desired format.")]
        /// <summary>
        /// Message template used to format incoming chat message into a desired format.
        /// </summary>
        public string messageTemplate;

        public Action_ReplyInChat() : base()
        {
            cooldown = 15;
            messageTemplate = defaultTemplate;
        }

        public Action_ReplyInChat(Action_ReplyInChat source) : base(source)
        {
            messageTemplate = source.messageTemplate;
        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
                TwitchManager.instance.WriteToChat(MessageFormatter.FormatMessage(messageTemplate, System.DateTime.Now, userID, userMessage)); // Time is NOW ; and not the time at which the viewer has sent his/her message.
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            base.DrawInspector();

            if (inspectorFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Reply template", EditorStyles.boldLabel);
                messageTemplate = EditorGUILayout.TextArea(messageTemplate);
                EditorGUILayout.SelectableLabel("Tags: " + defaultTemplate, EditorStyles.wordWrappedLabel);
                if (messageTemplate.Length > 0)
                {
                    EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                    EditorGUILayout.SelectableLabel(MessageFormatter.FormatMessage(messageTemplate, System.DateTime.Now, "Turkenberg", TurkenbergsMessage), EditorStyles.wordWrappedLabel);
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }
        }

        public override string InspectorName()
        {
            return "Reply in chat";
        }
#endif
    }
}
