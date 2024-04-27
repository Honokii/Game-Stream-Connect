using UnityEngine;
using TwitchConnect;

namespace TwitchConnect.Demo.GaltonTable
{
    public class BallTrigger : MonoBehaviour
    {
        public BoxCollider trigger;

        private void OnTriggerEnter(Collider other)
        {
            // we entered a pit ; check which pit it is
            GameManager gm = FindObjectOfType<GameManager>();

            gm.TriggerDetected(other as BoxCollider);
        }
    }
}
