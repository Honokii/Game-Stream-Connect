using UnityEngine;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Base class for all triggers.
    /// </summary>
    [System.Serializable]
    public abstract class Trigger : ScriptableObject, ITriggerable
    {
        #region Public variables

        [Tooltip("Is this trigger listening ?")]
        [SerializeField]
        /// <summary>
        /// Is this trigger listening ?
        /// </summary>
        public bool enabled;

        [HideInInspector]
        [SerializeField]
        public bool inspectorFoldout;

        /// <summary>
        /// Event raised when this trigger succesfully passes all checks.
        /// </summary>
        [SerializeField]
        public event OnTriggerSuccesful OnTriggerSuccesful;

        public Trigger()
        {
            enabled = true;
            inspectorFoldout = false;
        }

        public Trigger(Trigger source)
        {
            enabled = source.enabled;
            inspectorFoldout = source.inspectorFoldout;
            OnTriggerSuccesful = source.OnTriggerSuccesful;
        }
        #endregion

        #region ITriggerable interface
        public virtual void Enable()
        {
            enabled = true;
        }

        public virtual void Disable()
        {
            enabled = false;
        }

        public abstract void Initialize();

        public abstract void ReInitialize();

        public abstract void Deinitialize();

        public virtual void Update()
        {

        }

        protected virtual void OnTrigger(object sender, object payload, int userID, string userMessage)
        {
            if (!enabled)
                return;

            OnTriggerSuccesful.Invoke(sender, payload, userID, userMessage);
        }
        #endregion

#if UNITY_EDITOR
        public virtual string InspectorName()
        {
            return "Trigger";
        }

        public virtual void DrawInspector()
        {
            SerializedObject _this = new SerializedObject(this);
            ShowProperty(_this, "enabled");
        }

        internal void ShowProperty(SerializedObject _o, string _name)
        {
            EditorGUILayout.PropertyField(_o.FindProperty(_name));
            _o.ApplyModifiedProperties();
        }
#endif

        public virtual string GetCommandName()
        {
            return "";
        }
    }
}

