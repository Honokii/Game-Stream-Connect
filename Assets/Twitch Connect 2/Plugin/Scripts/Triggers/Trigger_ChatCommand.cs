using UnityEngine;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on given chat command.
    /// </summary>
    [System.Serializable]
    public class Trigger_ChatCommand : Trigger
    {
        [SerializeField]
        [Tooltip("The command that triggers this (case insensitive). To use a different command prefix, change command prefix in settings.")]
        /// <summary>
        /// The chat command that triggers this.
        /// </summary>
        public string command;

        public Trigger_ChatCommand()
        {
            command = "command";
        }

        public Trigger_ChatCommand(string _command)
        {
            command = _command;
        }

        public Trigger_ChatCommand(Trigger_ChatCommand source) : base(source)
        {
            command = source.command;
        }

        public override sealed void Initialize()
        {
            if (TwitchManager.instance != null && TwitchManager.instance.client != null && TwitchManager.instance.client.IsInitialized)
                TwitchManager.instance.client.OnChatCommandReceived += OnTrigger;
        }

        public override sealed void Deinitialize()
        {
            if (TwitchManager.instance != null)
            {
                if (TwitchManager.instance.client != null)
                    TwitchManager.instance.client.OnChatCommandReceived -= OnTrigger;
            }
        }

        public override void ReInitialize()
        {
            Initialize();
        }

        public virtual void OnTrigger(object sender, TwitchLib.Client.Events.OnChatCommandReceivedArgs e)
        {
            

            if (e.Command.CommandText.Equals(command, System.StringComparison.InvariantCultureIgnoreCase))
            {
                int userID = int.Parse(e.Command.ChatMessage.UserId);
                string userMessage;

                int index = e.Command.ChatMessage.Message.IndexOf(' ');

                if (index == -1)
                    userMessage = ""; // nothing written after command
                else
                    userMessage = e.Command.ChatMessage.Message.Substring(index + 1);

                base.OnTrigger(this, e, userID, userMessage);
            }
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                EditorGUI.indentLevel++;
                base.DrawInspector();
                ShowProperty(new SerializedObject(this), "command");
                EditorGUI.indentLevel--;
            }
            
        }

        public override string InspectorName()
        {
            return (TwitchManager.instance == null || TwitchManager.instance.settings == null) ? "Command: " + command : TwitchManager.instance.settings.commandPrefix + command;
        }
#endif

        public override string GetCommandName()
        {
            return enabled ? command : "";
        }
    }
}



