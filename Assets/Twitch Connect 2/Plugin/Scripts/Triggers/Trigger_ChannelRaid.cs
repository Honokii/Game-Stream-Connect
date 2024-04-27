using UnityEngine;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on given chat command.
    /// </summary>
    [System.Serializable]
    public class Trigger_ChannelRaid : Trigger
    {
        public Trigger_ChannelRaid()
        {
            
        }

        public Trigger_ChannelRaid(Trigger_ChatCommand source) : base(source)
        {
            
        }

        public override sealed void Initialize()
        {
            if (TwitchManager.instance != null && TwitchManager.instance.client != null && TwitchManager.instance.client.IsInitialized)
                TwitchManager.instance.client.OnRaidNotification += OnTrigger;
        }

        public override sealed void Deinitialize()
        {
            if (TwitchManager.instance != null)
            {
                if (TwitchManager.instance.client != null)
                    TwitchManager.instance.client.OnRaidNotification -= OnTrigger;
            }
        }

        public override void ReInitialize()
        {
            Initialize();
        }

        public virtual void OnTrigger(object sender, TwitchLib.Client.Events.OnRaidNotificationArgs e)
        {
            base.OnTrigger(this, e, int.Parse(e.RaidNotification.Login), "Channel is raided. Trigger not bound to specific user.");
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                EditorGUI.indentLevel++;
                base.DrawInspector();
                EditorGUI.indentLevel--;
            }
            
        }

        public override string InspectorName()
        {
            return "Channel raid";
        }
#endif
    }
}



