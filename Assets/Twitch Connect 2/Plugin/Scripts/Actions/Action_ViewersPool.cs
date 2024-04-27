using UnityEngine;
using System.Collections.Generic;
using TwitchLib.Client.Events;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Create and manage a pool of viewers.
    /// </summary>
    public class Action_ViewersPool : Action
    {
        [Tooltip("Maximum slots of the pool. Set to -1 for unlimited pool size.")]
        /// <summary>
        /// Maximum slots of the pool. Set to -1 for unlimited pool size.
        /// </summary>
        public int maxPoolSize; // -1 for unlimited

        /// <summary>
        /// Raw pool of viewers. Note: Viewers are sorted by order of entering the pool.
        /// </summary>
        public Queue<int> pool = new Queue<int>();

        internal Action_ViewersPool()
        {
            cooldown = 0; // by default, viewers can try to enter the pool as many times as they want..! However at least 5 seconds of cooldown should be fine for perf reasons. Modify at will.
        }

        internal Action_ViewersPool(Action_ViewersPool source) : base(source)
        {
            int[] p = new int[source.pool.Count];
            source.pool.CopyTo(p, 0);
            pool = new Queue<int>(p);
        }

        internal void AddViewer(int userID)
        {
            if (pool == null)
                pool = new Queue<int>();

            if (maxPoolSize > -1 && pool.Count > maxPoolSize)
            {
                Debug.Log("[Twitch Feature : Viewer pool] Pool is full, TY.");
                return;
            }

            if (pool.Contains(userID))
            {
                Debug.Log("[Twitch Feature : Viewer pool] Viewer " + UserDictionnary.GetUserName(userID) + " (id:" + userID + ") is already in pool.");
                return;
            }

            pool.Enqueue(userID);   
        }

        /// <summary>
        /// Empty the pool.
        /// </summary>
        /// <param name="alsoClearViewersHistory">By default also clears the history to avoid blocking some users from entering after clearing of pool.</param>
        public void ClearPool(bool alsoClearViewersHistory = true)
        {
            pool.Clear();
            // we also need to clear viewers history, assuming that clearing the pool means that anyone can retry to enter again thus ignoring previous history.
            // In some usecase (e.g. viewers can enter the pool at very specific time and any viewer has only one chance to enter), you can optionnally bypass history's clearing.
            if (alsoClearViewersHistory) viewersHistory.queue.Clear();
        }

        /// <summary>
        /// Retrieve the first viewer (the oldest viewer) in the pool but leaves it in the pool.
        /// </summary>
        /// <returns></returns>
        public int PeekNextViewer()
        {
            if (pool == null)
                return -1;

            if (pool.Count > 0)
                return pool.Peek();
            else
                return -1;
        }

        /// <summary>
        /// Extract and retrieve the first viewer (the oldest viewer) in the pool.
        /// </summary>
        /// <returns></returns>
        public int DequeueNextViewer()
        {
            if (pool == null)
                return -1;

            if (pool.Count > 0)
                return pool.Dequeue();
            else
                return -1;
        }

        /// <summary>
        /// Retrieve a random viewer in the pool but leaves it in the pool.
        /// </summary>
        /// <returns></returns>
        public int PeekRandomViewer()
        {
            if (pool == null)
                return -1;

            if (pool.Count > 0)
            {
                int index = Random.Range(0, pool.Count);
                return pool.ToArray()[index];
            }

            return -1;  
        }

        /// <summary>
        /// Extract and retrieve a random viewer in the pool.
        /// </summary>
        /// <returns></returns>
        public int DequeueRandomViewer()
        {
            if (pool == null)
                return -1;

            if (pool.Count > 1)
            {
                // deep copy of queue in same order
                int result;
                int poolCount = pool.Count;
                int index = Random.Range(0, poolCount);

                int[] queue = pool.ToArray();
                pool.Clear();

                result = queue[index];

                for (int i = 0; i < poolCount; i--)
                {
                    if (i != index)
                        pool.Enqueue(queue[i]);
                }

                return result;
            }
            else if (pool.Count == 0)
                return pool.Dequeue();

            return -1;
        }

        public override void Initialize()
        {
            base.Initialize();

            pool = new Queue<int>();
        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            // Can receive:
            // anyth really..!

            if (!opened)
            {
                return;
            }

            if (CanUseCommand(userID) && restriction.HasPrivilege(userID))
            {
                AddViewer(userID);
            }
        }
#if UNITY_EDITOR
        public override string InspectorName()
        {
            return "Viewers Pool";
        }

        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();
                EditorGUI.indentLevel++;
                SerializedObject o = new SerializedObject(this);
                ShowProperty(o, "maxPoolSize");
                EditorGUI.indentLevel--;

                if (pool == null)
                    return;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Number of viewers in pool:");
                EditorGUILayout.LabelField(pool.Count.ToString(), GUILayout.Width(60f));
                EditorGUILayout.EndHorizontal();
            }

        }
#endif
        public override void Deinitialize()
        {
            if (pool != null)
                pool.Clear();

            base.Deinitialize();
        }
    }
}