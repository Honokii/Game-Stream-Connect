using UnityEditor;

namespace TwitchConnect
{
    public class Trigger_NewFollower : Trigger
    {
        public Trigger_NewFollower() : base() { }

        public Trigger_NewFollower(Trigger_NewFollower source) : base(source) { }

        public override void Initialize()
        {
            if (TwitchManager.instance != null && TwitchManager.pubSub != null)
            {
                TwitchManager.pubSub.OnFollow += OnTrigger;
            }
        }

        public override void Deinitialize()
        {
            if (TwitchManager.pubSub != null)
            {
                TwitchManager.pubSub.OnFollow -= OnTrigger;
            }
        }

        public override void ReInitialize()
        {
            Deinitialize();
            Initialize();
        }

        public virtual void OnTrigger(object sender, TwitchLib.PubSub.Events.OnFollowArgs e)
        {
            UserDictionnary.AddToDictionnary(int.Parse(e.UserId), e.Username); // just in case this user never engaged in chat
            base.OnTrigger(this, e, int.Parse(e.UserId), "");
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
            return "New follower";
        }
#endif
    }
}

