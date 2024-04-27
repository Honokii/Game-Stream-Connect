using System.Collections.Generic;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Base class for all kind of options (bet, vote, etc.)
    /// </summary>
    public abstract class Option
    {
        public enum EOptionStatus
        {
            Unknown,
            Winning,
            Tie,
            Loosing
        }

        [Tooltip("Nice-name of the option.")]
        /// <summary>
        /// Nice-name of the option.
        /// </summary>
        public string name;

        [Tooltip("HashSet (unique list) of viewers that opted for this option.")]
        /// <summary>
        /// HashSet (unique list) of viewers that opted for this option.
        /// </summary>
        public HashSet<int> opters;

        /// <summary>
        /// Count of users that opted for this option.
        /// </summary>
        public int count { get { return opters.Count; } }

        [Tooltip("Result status of this option (winning, loosing, or tie).")]
        /// <summary>
        /// Result status of this option (winning, loosing, or tie).
        /// </summary>
        public EOptionStatus status;

        public Option(string _name)
        {
            name = _name;
            opters = new HashSet<int>();
            status = EOptionStatus.Unknown;
        }

        public void Register(int _userID)
        {
            opters.Add(_userID);
        }
    }
}

