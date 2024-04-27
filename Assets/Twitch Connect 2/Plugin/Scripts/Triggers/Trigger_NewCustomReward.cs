using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on a specific custom reward redeemal. Won't work for Twitch's built-in standard rewards.
    /// Rewards can be uniquely identified through their 'Title' as Twitch does not allow multiple custom rewards with the same title.
    /// Use this to trigger actions on a specific custom reward identified by its Title.
    /// Optionnaly a reward can expect a 'prompt', a text that the viewer enters when redeeming the custom reward.
    /// </summary>
    public class Trigger_NewCustomReward : Trigger_NewCustomReward_Any
    {
        [SerializeField]
        [Tooltip("The reward exact name that is used to identify the reward when redeemed by a viewer (case insensitive).")]
        /// <summary>
        /// The custom reward exact name on Twitch side that is used to identify the reward when redeemed by a viewer.
        /// </summary>
        public string rewardTitle;

        public Trigger_NewCustomReward() : base() { }

        public Trigger_NewCustomReward(string _rewardTitle) : base() { rewardTitle = _rewardTitle; }

        public override void OnChannelRewardRedeem(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e)
        {
            if (!enabled)
                return;

            if (string.Equals(e.RewardTitle, rewardTitle, System.StringComparison.InvariantCultureIgnoreCase))
                base.OnChannelRewardRedeem(this, e);
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();

                EditorGUI.indentLevel++;
                SerializedObject o = new SerializedObject(this);
                ShowProperty(o, "rewardTitle");
                EditorGUI.indentLevel--;
            }
        }

        public override string InspectorName()
        {
            return "Custom reward";
        }
#endif
    }
}

