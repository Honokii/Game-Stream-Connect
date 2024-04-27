using System;
using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    [RequireComponent(typeof(Collider))]
    [Serializable]
    public class Checkpoint : MonoBehaviour, IComparable<Checkpoint>
    {
        public int order;

        public int CompareTo(Checkpoint other)
        {
            return -other.order.CompareTo(order);
        }
    }
}

