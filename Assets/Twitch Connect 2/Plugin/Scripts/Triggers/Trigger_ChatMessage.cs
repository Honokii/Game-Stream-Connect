using UnityEngine;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on given received chat message (not a command).
    /// </summary>
    [System.Serializable]
    public class Trigger_ChatMessage : Trigger
    {
        [SerializeField]
        [Tooltip("The plain chat text that should trigger this. This will ignore command prefix and simply analyze the incoming chat messages (case insensitive).")]
        /// <summary>
        /// The plain chat text that should trigger this. This will ignore command prefix and simply analyze the incoming chat messages, sub messages, bits messages etc.</para>
        /// </summary>
        public string chatText;

        public enum EtextMode
        {
            Equals,
            Contains,
            StartsWith
        }

        [SerializeField]
        [Tooltip("How the trigger should look for text in message.")]
        /// <summary>
        /// How the trigger should look for text in message. </para>
        /// </summary>
        public EtextMode textDetectionMode;

        public Trigger_ChatMessage() : base()
        {
            chatText = "hi!";
        }

        public Trigger_ChatMessage(Trigger_ChatMessage source) : base(source)
        {
            chatText = source.chatText;
        }

        public override sealed void Initialize()
        {
            if (TwitchManager.instance != null && TwitchManager.instance.client.IsInitialized)
                TwitchManager.instance.client.OnMessageReceived += OnTrigger;
        }

        public override void Deinitialize()
        {
            if (TwitchManager.instance != null)
            {
                TwitchManager.instance.client.OnMessageReceived -= OnTrigger;
            }
        }

        public override void ReInitialize()
        {
            Initialize();
        }

        public virtual void OnTrigger(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            UserDictionnary.AddToDictionnary(sender, e);

            switch (textDetectionMode)
            {
                case EtextMode.Contains:
                    if (System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(e.ChatMessage.Message, chatText, System.Globalization.CompareOptions.IgnoreCase) >= 0)
                        base.OnTrigger(this, e, int.Parse(e.ChatMessage.UserId), e.ChatMessage.Message);
                    break;
                case EtextMode.StartsWith:
                    if (e.ChatMessage.Message.StartsWith(chatText, System.StringComparison.InvariantCultureIgnoreCase))
                        base.OnTrigger(this, e, int.Parse(e.ChatMessage.UserId), e.ChatMessage.Message); ;
                    break;
                default:
                    if (e.ChatMessage.Message.Equals(chatText, System.StringComparison.InvariantCultureIgnoreCase))
                        base.OnTrigger(this, e, int.Parse(e.ChatMessage.UserId), e.ChatMessage.Message);
                    break;
            }           
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                EditorGUI.indentLevel++;
                base.DrawInspector();
                ShowProperty(new SerializedObject(this), "textDetectionMode");
                ShowProperty(new SerializedObject(this), "chatText");
                EditorGUI.indentLevel--;
            }  
        }

        public override string InspectorName()
        {
            return "Chat Message: " + chatText;
        }
#endif
    }
}

