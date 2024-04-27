using System.Collections.Generic;
using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    // [CreateAssetMenu(fileName = "Tracks Set.asset", menuName = "Marble Game/Create new track set", order = 600)]
    public class TrackSet : ScriptableObject
    {
        [System.Serializable]
        public class TrackEntry
        {
            public string name;
            public GameObject prefabModule;
        }

        public List<TrackEntry> trackModules;
    }
}