using UnityEngine;
using UnityEditor;

namespace TwitchConnect.Demo.MarbleGame
{
    [CustomEditor(typeof(TrackManager))]
    public class TrackManagerEditor : Editor
    {
        static bool showTrackPath;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TrackManager manager = target as TrackManager;

            if (GUILayout.Button("Build track path"))
                manager.PrepareTrack();

            if (manager.modules != null && manager.modules.Count > 0)
            {
                if (GUILayout.Button(showTrackPath ? "Hide track path" : "Show track path"))
                {
                    showTrackPath = !showTrackPath;
                    SceneView.RepaintAll();
                }
                    

                if (GUILayout.Button("Clear track path"))
                    manager.ClearTrack();
            }

            EditorGUILayout.Space();

            DrawTrackBuilder();
        }

        private void DrawTrackBuilder()
        {
            TrackManager manager = target as TrackManager;

            if (manager.trackBuilderTrackSet == null || manager.trackBuilderTrackSet.trackModules == null || manager.trackBuilderTrackSet.trackModules.Count == 0)
                return;

            if (manager.modules.Count > 0 && GUILayout.Button("Erase last"))
            {
                manager.PrepareTrack();
                Undo.DestroyObjectImmediate(manager.modules[manager.modules.Count - 1].gameObject);
                manager.PrepareTrack();
            }

            EditorGUILayout.Space();

            foreach (TrackSet.TrackEntry te in manager.trackBuilderTrackSet.trackModules)
            {
                if (GUILayout.Button(te.name.Length > 0 ? te.name : te.prefabModule.name))
                {
                    manager.PrepareTrack();
                    AddTrackModule(te.prefabModule);
                }
            }
        }

        private void AddTrackModule(GameObject prefab)
        {
            TrackManager manager = target as TrackManager;

            GameObject result = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(result, "Track module added");
            result.transform.SetAsLastSibling();
            

            Transform t1P = result.transform; // <-- new GO
            Transform t1C = result.GetComponent<TrackModule>().entryHandle.transform; // <-- new GO's entry handle
            Transform t2 = manager.modules[manager.modules.Count - 1].exitHandle.transform; // <-- exitHandle

            TrackModule.Alignment(t1P, t1C, t2);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmosSelected(TrackManager manager, GizmoType gizmoType)
        {
            if (showTrackPath)
            {
                if (manager.checkpoints == null || manager.checkpoints.Count == 0)
                {
                    showTrackPath = false;
                    return;
                }

                for (int i = 0; i < manager.checkpoints.Count - 1; i++)
                {
                    Gizmos.color = Color.green;
                    try
                    {
                        Gizmos.DrawLine(manager.checkpoints[i].transform.position, manager.checkpoints[i + 1].transform.position);
                    }
                    catch (System.Exception)
                    {
                        manager.PrepareTrack();
                        return;
                    }
                }
            }
        }
    }
}