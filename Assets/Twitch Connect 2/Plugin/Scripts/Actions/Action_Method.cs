using UnityEngine.Events;
using UnityEditor;

namespace TwitchConnect
{
    public struct PersistentCall
    {

    }

    /// <summary>
    /// Call a given set of methods.
    /// </summary>
    [System.Serializable]
    public class Action_Method : Action
    {
        /// <summary>
        /// List of methods that will be called OnAction.
        /// </summary>
        //[UnityEngine.SerializeField]
        public OnMethod methods;

        //public List<ExposedReference<MonoBehaviour>> sceneMonos;
        [System.Serializable]
        public class OnMethod : UnityEvent<int, string, object> { }

        public Action_Method()
        {
            openOnStart = true;
        }

        public Action_Method(Action_Method source) : base(source)
        {
            methods = source.methods;
        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            if (!opened)
                return;

            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
                methods?.Invoke(userID, userMessage, payload);
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();
                EditorGUI.indentLevel++;
                ShowProperty(new SerializedObject(this), "methods");
                EditorGUI.indentLevel--;
            }
        }

        public override string InspectorName()
        {
            return "Call Methods";
        }
#endif
    }
}


