using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Trigger actions on bits donations.
    /// </summary>
    [System.Serializable]
    public class Trigger_BitsDonation : Trigger
    {
        [SerializeField]
        [Tooltip("Minimum (inclusive) amount of bits to raised this trigger.")]
        /// <summary>
        /// Minimum (inclusive) amount of bits to raised this trigger.
        /// </summary>
        public int minimumBitsAmount;

        [SerializeField]
        [Tooltip("Maximum (inclusive) amount of bits to raised this trigger.")]
        /// <summary>
        /// Maximum (inclusive) amount of bits to raised this trigger.
        /// </summary>
        public int maximumBitsAmount;

        public Trigger_BitsDonation()
        {
            minimumBitsAmount = 100;
            maximumBitsAmount = 200;
        }

        public Trigger_BitsDonation(int _minimumBitsAmount, int _maximumBitsAmount)
        {
            // make sure constructor avlues are coherent
            _minimumBitsAmount = System.Math.Max(_minimumBitsAmount, 0);
            _maximumBitsAmount = System.Math.Max(_maximumBitsAmount, 0);

            // invert order if needed
            minimumBitsAmount = System.Math.Min(_minimumBitsAmount, _maximumBitsAmount);
            maximumBitsAmount = System.Math.Max(_minimumBitsAmount, _maximumBitsAmount);
        }

        public Trigger_BitsDonation(Trigger_BitsDonation source) : base(source)
        {
            minimumBitsAmount = source.minimumBitsAmount;
            maximumBitsAmount = source.maximumBitsAmount;
        }

        public override void Initialize()
        {
            if (maximumBitsAmount < minimumBitsAmount)
            {
                int temp = minimumBitsAmount;
                minimumBitsAmount = maximumBitsAmount;
                maximumBitsAmount = temp;
            }
                

            if (TwitchManager.instance != null && TwitchManager.instance.client.IsInitialized)
                TwitchManager.instance.client.OnMessageReceived += OnTrigger;
        }

        public override void Deinitialize()
        {
            if (TwitchManager.instance != null && TwitchManager.instance.client != null)
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
            UserDictionnary.AddToDictionnary(int.Parse(e.ChatMessage.UserId), e.ChatMessage.DisplayName);

            if (e.ChatMessage.Bits >= 0 && e.ChatMessage.Bits >= minimumBitsAmount && e.ChatMessage.Bits <= maximumBitsAmount)
                base.OnTrigger(this, e, int.Parse(e.ChatMessage.UserId), e.ChatMessage.Message); // pss message including "cheer100" for example
        }


#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();
                SerializedObject o = new SerializedObject(this);
                ShowProperty(o, "minimumBitsAmount");
                ShowProperty(o, "maximumBitsAmount");
            }
        }

        public override string InspectorName()
        {
            return "Bits donation (" + minimumBitsAmount + " to " + maximumBitsAmount + ")";
        }

#endif
    }
}

