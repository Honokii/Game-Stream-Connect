using System;
using UnityEngine;

namespace TwitchConnect.Demo.MarbleGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class GameCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 5.0f;
        [Range(0f, 20f)] public float xSpeed = 40f;
        [Range(20f, 120f)] public float ySpeed = 120.0f;
        [Range(1f, 15f)] public float zSpeed = 10f;

        [Range(-90f, 0)] public float yMinLimit = -20f;
        [Range(0f, 90)] public float yMaxLimit = 80f;

        [Range(1f, 120f)] public float distanceMin = .5f;
        [Range(1f, 120f)] public float distanceMax = 15f;

        private Rigidbody rb;

        float x = 0.0f;
        float y = 0.0f;


        // Game manager
        GameManager game;

        // player manager
        PlayerManager players;

        private void Start()
        {
            //Offset = camTransform.position - Target.position;
            game = GameManager.instance;
            players = PlayerManager.instance;

            InitOrbitalCamera();
        }

        private void LateUpdate()
        {
            switch (game.state)
            {
                case GameManager.EGameState.NotReady:
                case GameManager.EGameState.Ready:
                    return;

                // show start zone
                case GameManager.EGameState.PlayersFillingIn:
                default:
                    target = players.startZoneTransform;
                    break;

                // show leader (list is ordered)
                case GameManager.EGameState.GameStartCountdown:
                case GameManager.EGameState.GameInProgress:
                    foreach (Player p in players.players)
                    {
                        if (p.state == Player.EPlayerState.Racing && p.mb.transform != null)
                        {
                            target = p.mb.transform;
                            break;
                        }
                    }
                    break;

                // show winner
                case GameManager.EGameState.Scoreboard:
                    if (game.scoreboard != null && game.scoreboard.Count > 0 && game.scoreboard[0].mb != null)
                        target = game.scoreboard[0].mb.transform;
                    else
                        target = players.startZoneTransform;
                    break;
            }

            OrbitalCameraView();
        }

        private void InitOrbitalCamera()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;

            rb = GetComponent<Rigidbody>();

            // Make the rigid body not change rotation
            if (rb != null)
            {
                rb.freezeRotation = true;
            }
        }

        private void OrbitalCameraView()
        {
            if (target)
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zSpeed, distanceMin, distanceMax);

                /*RaycastHit hit;
                if (Physics.Linecast(target.position, transform.position, out hit))
                {
                    distance -= hit.distance;
                }*/
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + target.position;

                transform.rotation = rotation;
                transform.position = position;
            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
}
}

