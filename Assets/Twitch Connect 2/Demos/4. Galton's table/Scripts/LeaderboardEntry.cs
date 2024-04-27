using System;

namespace TwitchConnect.Demo.GaltonTable
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string userName;
        public float score;
        public DateTime time;

        public LeaderboardEntry(string _userName, float _score)
        {
            userName = _userName;
            score = _score;
            time = DateTime.Now;
        }
    }
}

