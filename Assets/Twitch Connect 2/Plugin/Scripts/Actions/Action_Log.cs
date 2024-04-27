using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

namespace TwitchConnect
{

    /// <summary>
    /// Call a given set of methods.
    /// </summary>
    [System.Serializable]
    public class Action_Log : Action
    {
        public enum ELogType
        {
            Basic = 0,
            UserId = 1,
            UserIdAndMessage = 2
        }

        /// <summary>
        /// Level of information to log in console when action is fired.
        /// </summary>
        [Tooltip("Level of information to log in console when this action is fired.")]
        public ELogType logType;

        /// <summary>
        /// Include the type of trigger that fired this action.
        /// </summary>
        [Tooltip("Include the type of trigger that fired this action.")]
        public bool identifyTriggerType;

        /// <summary>
        /// Optional text object to also output logs.
        /// </summary>
        [Tooltip("Output to text object instead of console.")]
        public Text outputText;

        /// <summary>
        /// Add carriage return at end of line.
        /// </summary>
        [Tooltip("Add carriage return at end of line.")]
        public bool carriageReturn;

        public Action_Log()
        {
            openOnStart = true;
            logType = ELogType.UserId;
            identifyTriggerType = true;
            carriageReturn = true;
            outputText = null;
        }

        public Action_Log(Action_Log source) : base(source)
        {
            logType = source.logType;
            outputText = source.outputText;
            identifyTriggerType = source.identifyTriggerType;
            carriageReturn = source.carriageReturn;
        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            if (!opened)
                return;

            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
            {
                string output = "Action triggered";

                if (identifyTriggerType)
                    output += " from " + sender.GetType();

                switch (logType)
                {
                    case ELogType.UserId:
                        output += " by user " + userID.ToString() + '.';
                        break;
                    case ELogType.UserIdAndMessage:
                        output += " by user " + userID.ToString() + " saying " + userMessage + '.';
                        break;
                    default:
                        output += '.';
                        break;
                }

                if (carriageReturn)
                    output += '\r';

                if (outputText != null)
                    outputText.text += output;
                else
                    Debug.Log(output);
            }
                
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                EditorGUI.indentLevel++;
                SerializedObject so = new SerializedObject(this);
                EditorGUILayout.Space();
                ShowProperty(so, "logType");
                ShowProperty(so, "identifyTriggerType");
                ShowProperty(so, "outputText");
                if (outputText != null) ShowProperty(so, "carriageReturn");
                EditorGUI.indentLevel--;
            }
        }

        public override string InspectorName()
        {
            return "Log";
        }
#endif

    }
}


