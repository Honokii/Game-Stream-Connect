using UnityEditor;
using UnityEngine;


namespace TwitchConnect
{
    [CustomEditor(typeof(TwitchChatReceiver))]
    public class TwitchChatInspector : Editor
    {
        // Auto-format
        private SerializedProperty b_autoFormatMessages;
        private SerializedProperty s_customMessageFormat;
        private SerializedProperty b_endCharCr;
        private SerializedProperty b_endCharNl;
        private SerializedProperty s_customEndOfMessage;
        private SerializedProperty s_lastFormattedMessage;


        // Update target
        private SerializedProperty b_targetUpdate;
        private SerializedProperty b_textTargetOnSameGameObject;
        private SerializedProperty text_targetText;
        // private SerializedProperty tmp_targetTMP; // not tested
        // private SerializedProperty o_targetObject; // not tested

        private void OnEnable()
        {
            // Auto-format
            b_autoFormatMessages = serializedObject.FindProperty("autoFormatMessages");
            s_customMessageFormat = serializedObject.FindProperty("customMessageFormat");
            b_endCharCr = serializedObject.FindProperty("endCharCr");
            b_endCharNl = serializedObject.FindProperty("endCharNl");
            s_customEndOfMessage = serializedObject.FindProperty("customEndOfMessage");
            s_lastFormattedMessage = serializedObject.FindProperty("lastFormattedMessage");

            // Update target
            b_targetUpdate = serializedObject.FindProperty("targetUpdate");
            b_textTargetOnSameGameObject = serializedObject.FindProperty("textTargetOnSameGameObject");
            text_targetText = serializedObject.FindProperty("targetText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(b_autoFormatMessages);
            EditorGUILayout.PropertyField(s_customMessageFormat);
            EditorGUILayout.HelpBox(new GUIContent("Tags supported: ##SENDER## , ##MESSAGE## and ##TIME##"));
            EditorGUILayout.PropertyField(b_endCharCr);
            EditorGUILayout.PropertyField(b_endCharNl);
            EditorGUILayout.PropertyField(s_customEndOfMessage);
            EditorGUILayout.PropertyField(s_lastFormattedMessage);

            EditorGUILayout.PropertyField(b_targetUpdate);
            if (b_targetUpdate.boolValue)
            {
                EditorGUILayout.PropertyField(b_textTargetOnSameGameObject);
                if (!b_textTargetOnSameGameObject.boolValue)
                {
                    EditorGUILayout.PropertyField(text_targetText);
                }
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
