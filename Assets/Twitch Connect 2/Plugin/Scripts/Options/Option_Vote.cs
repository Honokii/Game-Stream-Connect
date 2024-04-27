using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Defines a vote.
    /// </summary>
    [System.Serializable]
    public class Option_Vote : Option
    {
        [Tooltip("Argument used in chat for viewers to vote for this vote option.")]
        /// <summary>
        /// Argument used in chat for viewers to vote for this vote option.
        /// </summary>
        public string argument;

        public Option_Vote(string _name, string _argument) : base(_name)
        {
            argument = _argument;
        }


    }
}