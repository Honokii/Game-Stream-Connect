using System;
using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    [ExecuteInEditMode]
    public class TrackModule : MonoBehaviour, IComparable<TrackModule>
    {
        public TrackHandle entryHandle;
        public TrackHandle exitHandle;

        public int index;

        public Checkpoint[] checkpoints;

        public TrackModule nextModule;
        public TrackModule previousModule;

        public TrackModule moduleToSnapTo;

        public void FindAdjacentModules()
        {
            nextModule = FindAdjacentTrackModule(TrackHandle.ETrackHandleType.Entry);
            previousModule = FindAdjacentTrackModule(TrackHandle.ETrackHandleType.Exit);
        }

        public void OrderModule(ref int currentModuleOrder)   
        {
            index = currentModuleOrder;
            currentModuleOrder++;

            if (nextModule != null)
            {
                nextModule.OrderModule(ref currentModuleOrder);
            }            
        }

        private TrackModule FindAdjacentTrackModule(TrackHandle.ETrackHandleType type)
        {
            Collider[] hits;

            switch (type)
            {
                case TrackHandle.ETrackHandleType.Entry:
                    hits = Physics.OverlapSphere(exitHandle.transform.position, exitHandle.col.radius);
                    break;
                case TrackHandle.ETrackHandleType.Exit:
                    hits = Physics.OverlapSphere(entryHandle.transform.position, entryHandle.col.radius);
                    break;
                default:
                    return null;
            }

            if (hits.Length > 0)
            {
                foreach (Collider hit in hits)
                {
                    TrackHandle handle = hit.gameObject.GetComponent<TrackHandle>();

                    if (handle != null && handle.type == type && handle.GetParent() != this)
                    {
                        return handle.GetParent();
                    } 

                }
            }

            return null;
        }

        public void AssignOrderToCheckpoints(ref int startOrder, int step)
        {
            // assign to each checkpoint in order
            foreach (Checkpoint cp in checkpoints)
            {
                cp.order = startOrder;
                startOrder += step;
            }
        }

        public int CompareTo(TrackModule other)
        {
            return index - other.index;
        }

        public void Snap()
        {
            if (moduleToSnapTo == null)
                return;

            Alignment(transform, entryHandle.transform, moduleToSnapTo.exitHandle.transform);
        }

        public static void Alignment(Transform t1P, Transform t1C, Transform t2)
        {
            t1P.rotation = t2.rotation * Quaternion.Inverse(t1C.localRotation);
            t1P.position = t2.position + (t1P.position - t1C.position);
        }

        void OnDrawGizmosSelected()
        {
            if (entryHandle == null || exitHandle == null)
                return;

            if (entryHandle.GetParent() == null)
                return;

            moduleToSnapTo = FindAdjacentTrackModule(TrackHandle.ETrackHandleType.Exit);

            // if found one
            if (moduleToSnapTo != null)
            {
                // indicate that there is a possible clip
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(moduleToSnapTo.exitHandle.transform.position, moduleToSnapTo.exitHandle.col.radius);
            }
            else
            {
                // else just show the check zone
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(entryHandle.transform.position, entryHandle.col.radius);
            }
        }
    }
}