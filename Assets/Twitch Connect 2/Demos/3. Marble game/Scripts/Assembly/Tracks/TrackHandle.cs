using UnityEngine;


namespace TwitchConnect.Demo.MarbleGame
{
    [RequireComponent(typeof(SphereCollider))]
    public class TrackHandle : MonoBehaviour
    {
        public enum ETrackHandleType
        {
            Entry,
            Exit
        }

        public ETrackHandleType type;
        private TrackModule parentModule;
        public SphereCollider col;

        public TrackModule GetParent()
        {
            parentModule = GetComponentInParent<TrackModule>();

            if (col == null)
            {
                Debug.LogWarning("Error in Handle definition, missing collider.");
            }

            gameObject.layer = 6;

            return parentModule;
        }
    }
}

