using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Base class for all actions.
    /// <para>All actions share the following features:</para>
    /// <para>- Cooldown system, to avoid viewers invoking an action too many times, or even only once.</para>
    /// <para>- Viewers history to support the above mentionned cooldown system.</para>
    /// </summary>
    [System.Serializable]
    public class Action : ScriptableObject, IActionable, IViewersHistory
    {
        #region Public variables
        [Tooltip("Per-viewer time during which a viewer is prevented from triggering this again. Set to -1 to allow viewers to trigger this only once until history is cleared. Set to 0 to let viewers trigger this as many times as they want.")]
        /// <summary>
        /// Per-viewer time during which a viewer is prevented from triggering this again. Set to -1 to allow viewers to trigger this only once until history is cleared. Set to 0 to let viewers trigger this as many times as they want.
        /// </summary>
        public float cooldown;

        [Tooltip("Should this Action be opened right on start ?")]
        /// <summary>
        /// Should this Action be opened right on start ?
        /// </summary>
        public bool openOnStart;

        [Tooltip("Is this Action currently opened ?")]
        /// <summary>
        /// Is this Action currently opened ?
        /// </summary>
        public bool opened;

        [Tooltip("(Optional) Timer after which the action will automatically close. On Action reset, this value will be used again (see remaining time value). Set to 0 to never expire.")]
        /// <summary>
        /// (Optional) Timer after which the action will automatically close. On Action reset, this value will be used again (see remaining time value). Set to 0 to never expire.
        /// </summary>
        public float duration;

        [Tooltip("Remaining time for this action (if duration is set above 0). Ignored otherwise. When the timer reaches 0, this action closes automatically).")]
        /// <summary>
        /// Remaining time for this action (if duration is set above 0). Ignored otherwise. When the timer reaches 0, this action closes automatically).
        /// </summary>
        public float remainingTime;

        [HideInInspector]
        public bool inspectorFoldout;
        [HideInInspector]
        public bool restrictionFoldout;
        [HideInInspector]
        public Restriction restriction;

        #endregion

        public Action()
        {
            cooldown = 0f;
            openOnStart = true;
            opened = false;
            duration = 0f;
            remainingTime = 0f;
            inspectorFoldout = false;
            restrictionFoldout = false;
            restriction = new Restriction();
            viewersHistory = new ViewersHistory();
        }

        public Action(Action source)
        {
            cooldown = source.cooldown;
            openOnStart = source.openOnStart;
            opened = source.opened;
            duration = source.duration;
            remainingTime = source.remainingTime;
            inspectorFoldout = source.inspectorFoldout;
            restrictionFoldout = source.restrictionFoldout;
            restriction = source.restriction;
            viewersHistory = new ViewersHistory(); // ignore history as its only used in runtime. Implement copy constructor in ViewersHistory if needed otherwise.
        }

        #region IActionable interface
        public virtual void Initialize()
        {
            remainingTime = duration;
            opened = openOnStart;

            InitializeHistory();
        }

        public virtual void Deinitialize()
        {
            remainingTime = duration;
            opened = false;

            InitializeHistory();
        }

        public virtual void Update()
        {
            if (!opened)
            {
                return;
            }

            UpdateHistory(); // use a viewersHistory just in case we override the triggers controls

            if (duration <= 0)
            {
                // Skip if timer is set to 0
                return;
            }

            remainingTime -= Time.deltaTime;

            if (remainingTime > 0f)
                opened = true;
            else
            {
                remainingTime = 0f;
                opened = false;
            }

        }

        public virtual void OnAction(object sender, object payload, int userID, string userMessage)
        {
            
        }
        #endregion
#if UNITY_EDITOR
        public virtual void DrawInspector()
        {
            SerializedObject o = new SerializedObject(this);

            if (inspectorFoldout)
            {

                EditorGUI.indentLevel++;

                ShowProperty(o, "cooldown");
                ShowProperty(o, "openOnStart");
                ShowProperty(o, "opened");
                ShowProperty(o, "duration");
                ShowProperty(o, "remainingTime");

                restrictionFoldout = EditorGUILayout.Foldout(restrictionFoldout, "Restrictions (" + restriction.count + ")", true);

                if (restrictionFoldout)
                {
                    EditorGUI.indentLevel++;

                    ShowProperty(o, "restriction.broadcaster", "Broadcaster");
                    ShowProperty(o, "restriction.vips", "VIPs");
                    ShowProperty(o, "restriction.moderators", "Moderators");
                    ShowProperty(o, "restriction.admins", "Admins");
                    ShowProperty(o, "restriction.global_mods", "Global mods");

                    //EditorGUILayout.LabelField("Subs-only is WIP");

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
            }
        }

        public virtual string InspectorName()
        {
            return "Action";
        }

        internal void ShowProperty(SerializedObject _o, string _name)
        {
            SerializedProperty p = _o.FindProperty(_name);
            EditorGUILayout.PropertyField(p);
            _o.ApplyModifiedProperties();
        }

        internal void ShowProperty(SerializedObject _o, string _name, string label)
        {
            SerializedProperty p = _o.FindProperty(_name);
            EditorGUILayout.PropertyField(p, new GUIContent(label));
            _o.ApplyModifiedProperties();
        }
#endif

#region IViewersHistory interface
        internal ViewersHistory viewersHistory;

        public virtual void InitializeHistory()
        {
            viewersHistory = new ViewersHistory();
        }

        /// <summary>
        /// Manually clear history. It forgets all users that triggered this action, allowing them to trigger it again.
        /// </summary>
        public virtual void ClearHistory()
        {
            if (viewersHistory != null)
                viewersHistory.queue.Clear();
        }

        public virtual bool CanUseCommand(int _id, bool AddToHistory = true)
        {
            if (viewersHistory == null) // should not happen
                return false;

            if (_id == -1) // first make sure we actually have an ID. -1 = don't check (intentional).
                return true;

            if (cooldown == 0f) // infinite post ; can always post
                return true;

            if (viewersHistory.Contains(_id)) // known user ; so cannot post
                return false;


            if (AddToHistory) // so the viewer can trigger ; add him/she to history.
                viewersHistory.Add(_id);

            return true; // finally, returns true
        }

        public virtual void UpdateHistory()
        {
            if (viewersHistory != null && cooldown > 0f)
                viewersHistory.Update(Time.timeSinceLevelLoad - cooldown);
        }
        #endregion
    }
}

