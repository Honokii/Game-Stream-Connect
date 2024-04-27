namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on New subscriber, Re-subscribe or Gifter subscriber.
    /// </summary>
    [System.Serializable]
    public class Trigger_ViewerLeft : Trigger
    {
        public Trigger_ViewerLeft() : base()
        { }


        public Trigger_ViewerLeft(Trigger_ViewerLeft source) : base(source)
        { }

        public override void Initialize()
        {
            if (TwitchManager.instance != null)
            {
                TwitchManager.instance.watchForViewers = true;
                TwitchManager.instance.OnViewerLeft += OnTrigger;
            }

        }

        public override void Update()
        {
            if (TwitchManager.instance != null)
            {
                if (TwitchManager.instance.watchForViewers == true && TwitchManager.instance.viewerWatcherCoroutineStarted == false)
                    TwitchManager.instance.StartCoroutine(TwitchManager.instance.UpdateKnownViewers());
            }
        }

        public override void ReInitialize()
        {
            Deinitialize();
            Initialize();
        }

        public override void Deinitialize()
        {
            if (TwitchManager.instance != null)
            {
                TwitchManager.instance.OnViewerLeft -= OnTrigger;
            }
        }

        public virtual void OnTrigger(int userID)
        {
            // just propagate the trigger
            base.OnTrigger(this, null, userID, "Viewer left - no message");
        }


#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
                base.DrawInspector();
        }

        public override string InspectorName()
        {
            return "Viewer left";
        }
#endif
    }
}
