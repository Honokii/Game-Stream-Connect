using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Defines a bet.
    /// </summary>
    [System.Serializable]
    public class Option_Bet : Option
    {
        [Tooltip("Numerical value of this bet option.")]
        /// <summary>
        /// Numerical value of this bet option.
        /// </summary>
        public float value;

        public Option_Bet(float _value, string _argument) : base(_argument)
        {
            value = _value;
        }

        public Option_Bet(float _value) : base(_value.ToString())
        {
            value = _value;
        }
    }
}

