using UnityEngine;
using System.Collections.Generic;

namespace TwitchConnect
{
    /// <summary>
    /// Internal class to save and load features set.
    /// </summary>
    //[CreateAssetMenu(menuName = "TwitchConnect/Feature set")]
    public class FeatureSet : ScriptableObject
    {
        [SerializeField]
        public List<Feature> features;

        public FeatureSet()
        {
            features = new List<Feature>();
        }
    }
}