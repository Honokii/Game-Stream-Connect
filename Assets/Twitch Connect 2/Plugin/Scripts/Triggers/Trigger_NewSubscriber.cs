using System;
using TwitchLib.Client.Events;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on New subscriber, Re-subscribe or Gifter subscriber.
    /// </summary>
    [System.Serializable]
    public class Trigger_NewSubscriber : Trigger
    {
        public bool subscriber;
        public bool reSubscriber;
        public bool primeSubscriber;
        public bool giftedSubscriber;


        public Trigger_NewSubscriber() : base()
        {
            subscriber = true;
            reSubscriber = true;
            primeSubscriber = true;
            giftedSubscriber = true;
        }


        public Trigger_NewSubscriber(Trigger_NewSubscriber source) : base(source)
        { }

        public override void Initialize()
        {
            // subscribe this event to the correct client event.
            if (TwitchManager.instance != null && TwitchManager.instance.client != null)
            {
                if (subscriber)
                {
                    TwitchManager.instance.client.OnNewSubscriber += OnNewSubscriber;
                }

                if (reSubscriber)
                {
                    TwitchManager.instance.client.OnReSubscriber += OnReSubscriber;
                }

                if (primeSubscriber)
                {
                    TwitchManager.instance.client.OnPrimePaidSubscriber += OnPrimePaidSubscriber; ;
                }

                if (giftedSubscriber)
                {
                    TwitchManager.instance.client.OnGiftedSubscription += OnGiftedSubscription; ;
                }
            }
        }

        private void OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            if (!enabled)
                return;

            UserDictionnary.AddToDictionnary(int.Parse(e.GiftedSubscription.UserId), e.GiftedSubscription.DisplayName);

            base.OnTrigger(this, e, int.Parse(e.GiftedSubscription.UserId), string.Empty);
        }

        private void OnPrimePaidSubscriber(object sender, OnPrimePaidSubscriberArgs e)
        {
            if (!enabled)
                return;

            UserDictionnary.AddToDictionnary(int.Parse(e.PrimePaidSubscriber.UserId), e.PrimePaidSubscriber.DisplayName);

            base.OnTrigger(this, e, int.Parse(e.PrimePaidSubscriber.UserId), e.PrimePaidSubscriber.ResubMessage);
        }

        private void OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            if (!enabled)
                return;

            UserDictionnary.AddToDictionnary(int.Parse(e.ReSubscriber.UserId), e.ReSubscriber.DisplayName);

            base.OnTrigger(this, e, int.Parse(e.ReSubscriber.UserId), e.ReSubscriber.ResubMessage);
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (!enabled)
                return;

            UserDictionnary.AddToDictionnary(int.Parse(e.Subscriber.UserId), e.Subscriber.DisplayName);

            base.OnTrigger(this, e, int.Parse(e.Subscriber.UserId), e.Subscriber.ResubMessage);
        }

        public override void Deinitialize()
        {
            if (TwitchManager.instance != null && TwitchManager.instance.client != null)
            {
                if (subscriber)
                {
                    TwitchManager.instance.client.OnNewSubscriber -= OnNewSubscriber;
                }

                if (reSubscriber)
                {
                    TwitchManager.instance.client.OnReSubscriber -= OnReSubscriber;
                }

                if (primeSubscriber)
                {
                    TwitchManager.instance.client.OnPrimePaidSubscriber -= OnPrimePaidSubscriber; ;
                }

                if (giftedSubscriber)
                {
                    TwitchManager.instance.client.OnGiftedSubscription -= OnGiftedSubscription; ;
                }
            }
        }

        public override void ReInitialize()
        {
            Initialize();
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();

                SerializedObject o = new SerializedObject(this);
                ShowProperty(o, "subscriber");
                ShowProperty(o, "reSubscriber");
                ShowProperty(o, "primeSubscriber");
                ShowProperty(o, "giftedSubscriber");
            }
               


        }

        public override string InspectorName()
        {
            return "New subscriber";
        }
#endif
    }
}
