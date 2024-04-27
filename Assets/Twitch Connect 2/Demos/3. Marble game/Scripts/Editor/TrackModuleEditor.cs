using UnityEngine;
using UnityEditor;

namespace TwitchConnect.Demo.MarbleGame
{
    [CustomEditor(typeof(TrackModule))]
    [CanEditMultipleObjects]
    public class TrackModuleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Inverse direction"))
            {
                InverseDirection();
            }

            if (GUILayout.Button("Snap"))
            {
                Snap();
            }
        }

        private void Snap()
        {
            TrackModule module = (TrackModule)target;

            if (Selection.count == 1)
            {
                module.Snap();
            }
/*            else if (Selection.count > 1)
            {
                TrackModule tm;
                List<TrackModule> modules = new List<TrackModule>();
                foreach (GameObject go in Selection.gameObjects)
                {
                    tm = go.GetComponent<TrackModule>();
                    if (tm != null)
                    {
                        modules.Add(tm);
                        module.Snap();
                    }
                        
                }                
            }*/
            
        }

        private void InverseDirection()
        {
            TrackModule module = (TrackModule)target;

            if (module.entryHandle == null || module.exitHandle == null)
                return;

            TrackHandle entryHandle = module.entryHandle;

            module.entryHandle = module.exitHandle;
            module.exitHandle = entryHandle;

            module.entryHandle.transform.Rotate(Vector3.up, 180f, Space.Self);
            module.exitHandle.transform.Rotate(Vector3.up, 180f, Space.Self);

            module.entryHandle.gameObject.name = "Entry";
            module.exitHandle.gameObject.name = "Exit";

            foreach (Checkpoint cp in module.checkpoints)
            {
                cp.transform.Rotate(Vector3.up, 180f, Space.Self);
            }

            System.Array.Reverse(module.checkpoints);
        }
    }
}

