using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Defines a Twitch viewer (taken from Twitch Connect v1).
    /// </summary>
    [System.Serializable]
    public class TwitchViewer
    {


        public string name;
        public int id;
        public float timestamp;

        public TwitchViewer(string _name, int _id)
        {
            name = _name;
            id = _id;
            timestamp = Time.timeSinceLevelLoad;
        }
    }
}
