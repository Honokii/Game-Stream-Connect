using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    public class AddForceToRigidbodies : MonoBehaviour
    {
        public bool compensateGravity;
        [Range(0f, 20f)] public float elevatorUpSpeed = 10f;

        public bool limitSpeed = true;
        public float speedLimit = 10f;

        private void OnTriggerStay(Collider other)
        {
            PlayerMonobehaviour pmb = other.GetComponent<PlayerMonobehaviour>();

            if (pmb == null || pmb.rb == null)
                return;

            Vector3 force = compensateGravity ? -Physics.gravity : Vector3.zero;
            force += elevatorUpSpeed * transform.up;

            pmb.rb.AddForce(force * pmb.rb.mass, ForceMode.Force);

            if (pmb.rb.velocity.sqrMagnitude > speedLimit * speedLimit)
            {
                pmb.rb.velocity = Vector3.ClampMagnitude(pmb.rb.velocity, speedLimit);
            }
        }
    }
}

