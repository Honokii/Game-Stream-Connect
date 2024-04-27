using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    /// <summary>
    /// Simply prepare all checkpoints for the game.
    /// </summary>
    public class TrackManager : MonoBehaviour
    {
        public List<TrackModule> modules;

        //[HideInInspector]
        public List<Checkpoint> checkpoints;

        public int checkpointStep = 100;
        public int currentCheckpointIndex;
        public int currentModuleIndex;

        public TrackSet trackBuilderTrackSet;

        private void Start()
        {
            PrepareTrack();
        }

        public void PrepareTrack()
        {
            // get all track modules
            modules = new List<TrackModule>(FindObjectsOfType<TrackModule>());
            checkpoints = new List<Checkpoint>();

            if (modules.Count == 0)
            {
                Debug.Log("No track modules found.");
                return;
            }

            List<TrackModule> modulesToRemove = new List<TrackModule>();
            foreach (TrackModule tm in modules)
            {
                if (!tm.gameObject.activeInHierarchy)
                {
                    modulesToRemove.Add(tm);
                }
                else
                {
                    tm.FindAdjacentModules();

                    if (tm.nextModule == null && tm.previousModule == null && modules.Count > 1)
                    {
                        Debug.Log("A track module not connected to anything has been detected.");
                        modulesToRemove.Add(tm);
                    }
                }
            }

            foreach (TrackModule tmtr in modulesToRemove)
            {
                modules.Remove(tmtr);
            }

            // resuse this list for unconnected modules.
            modulesToRemove.Clear();
            currentModuleIndex = 0;
            // now just make sure we only have one track module that does not have previous module
            foreach (TrackModule tm in modules)
            {
                if (tm.previousModule == null)
                {
                    modulesToRemove.Add(tm);
                } 
            }

            if (modulesToRemove.Count > 1)
            {
                Debug.LogWarning("Multiple track module with unconnected entry, cannot determine first module and compute track order.");
                foreach (TrackModule tm in modulesToRemove)
                {
                    Debug.LogWarning(tm.gameObject.name + "has no connected entry. Is it the first one ?");
                }
                return;
            }
            else if (modulesToRemove.Count == 0)
            {
                Debug.LogWarning("Track seems to be circular. Please move the entry handle of the module you want to start with.");
            }
            else
            {
                modulesToRemove[0].OrderModule(ref currentModuleIndex);
                modules.Sort();
            }

            currentCheckpointIndex = 0;
            foreach (TrackModule tm in modules) // this is sorted
            {
                tm.AssignOrderToCheckpoints(ref currentCheckpointIndex, checkpointStep);

                foreach (Checkpoint cp in tm.checkpoints)
                {
                    checkpoints.Add(cp);
                }
            }

            for (int i = 0; i < checkpoints.Count - 1; i++)
            {
                checkpoints[i].gameObject.tag = "Checkpoint";
            }
            
            if (checkpoints.Count > 0)
                checkpoints[checkpoints.Count-1].gameObject.tag = "FinishLine";

            // revert checkpoint index minus step to have index of last checkpoint
            currentCheckpointIndex -= checkpointStep;
            currentCheckpointIndex = Mathf.Max(0, currentCheckpointIndex);
        }

        public void ClearTrack()
        {
            modules.Clear();
            checkpoints.Clear();
            currentCheckpointIndex = 0;
            currentModuleIndex = 0;
        }
    }
}


