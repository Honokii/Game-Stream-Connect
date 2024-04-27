using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on ANY custom reward redeemal. Won't work for Twitch's built-in standard rewards.
    /// Rewards can be uniquely identified through their 'Title' as Twitch does not allow multiple custom rewards with the same title.
    /// Optionnaly a reward can expect a 'prompt', a text that the viewer enters when redeeming the custom reward.
    /// </summary>
    public class Trigger_NewCustomReward_Any : Trigger
    {
        public Trigger_NewCustomReward_Any() : base() { }

        public Trigger_NewCustomReward_Any(Trigger_NewCustomReward source) : base(source) { }

        public override void Initialize()
        {
            if (TwitchManager.instance != null)
            {
                TwitchManager.instance.OnChannelRewardRedeem += OnChannelRewardRedeem;
            }
        }

        public override void Deinitialize()
        {
            if (TwitchManager.instance != null)
            {
                TwitchManager.instance.OnChannelRewardRedeem -= OnChannelRewardRedeem;
            }
        }

        public override void ReInitialize()
        {
            Deinitialize();
            Initialize();
        }

        public virtual void OnChannelRewardRedeem(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e)
        {
            if (!enabled)
                return;

            int userID = UserDictionnary.GetUserIDFromUsername(e.Login);

            if (userID < 0 && TwitchManager.instance.api != null)
            {
                GameObject obj = new GameObject("Requesting userID for channel reward");
                obj.AddComponent<ChannelRewardUserIDTwitchRequest>().StartTwitchRequest(sender, e, this);
            }
            else
            {
                base.OnTrigger(this, e,
                userID,
                e.RewardTitle + " - " + e.Message);
            }
        }

        internal void OnChannelRewardCallback(object sender, int userID, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e)
        {
            base.OnTrigger(this, e,
                userID,
                e.RewardTitle + " - " + e.Message);
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
            return "Any custom reward";
        }
#endif
    }
}

