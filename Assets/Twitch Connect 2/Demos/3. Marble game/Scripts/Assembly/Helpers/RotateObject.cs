using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    public class RotateObject : MonoBehaviour
    {
        private enum ERotationAxis
        {
            X,Y,Z
        }

        [SerializeField]
        private ERotationAxis axis;
        [SerializeField]
        [Range(0f,50f)]
        private float rotationSpeed = 5f;

        // Update is called once per frame
        void FixedUpdate()
        {
            switch (axis)
            {
                case ERotationAxis.X:
                    transform.Rotate(rotationSpeed * Time.fixedDeltaTime, 0f, 0f, Space.Self);
                    break;

                case ERotationAxis.Y:
                    transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f, Space.Self);
                    break;

                case ERotationAxis.Z:
                    transform.Rotate(0f, 0f, rotationSpeed * Time.fixedDeltaTime, Space.Self);
                    break;

                default:
                    break;


            }
        }
    }
}


