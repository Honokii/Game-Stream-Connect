using System;
using UnityEngine;
using UnityEngine.UI;

namespace TwitchConnect.Demo.MarbleGame
{
    public class PlayerMonobehaviour : MonoBehaviour
    {
        /// <summary>
        /// Context of this player's gameObject, containing metadata such as userId, username, race times etc.
        /// </summary>
        [HideInInspector]
        public Player player;

        [Header("Components")]
        public Rigidbody rb;
        public Collider col;
        public Collider bounceCollider;
        public TrailRenderer trail;

        [Header("Name tag")]
        public string viewerName = "Viewer's name"; // name of the viewer ; by default it's 'Viewer's name' for testing purposes:
        public int viewerId = -1; // name of the viewer ; by default it's 'Viewer's name' for testing purposes:

        [Header("Skins")]
        [SerializeField] private Material botMaterial;
        [SerializeField] private Material botTrailMaterial;

        [Header("Name display")]
        
        [SerializeField] private Text nameTagText; // To update content of name tag:
        [SerializeField] private Transform nameTagCanvasTransform; // position the canvas vertically above viewer at all time:
        [SerializeField] [Range(0.01f, 1f)] private float nameTagHeight = 0.3f;// Height of te canvas relative to viewer's base transform

        private void Start()
        {
            nameTagText.text = viewerName;

            if (player != null && player.type == Player.EPlayerType.Bot)
            {
                GetComponent<MeshRenderer>().material = botMaterial;
                trail.material = botTrailMaterial;
            }    
        }

        private void FixedUpdate()
        {
            
        }

        private void LateUpdate()
        {
            nameTagCanvasTransform.position = transform.position + nameTagHeight * Vector3.up;
            nameTagCanvasTransform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        #region public methods
        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }
        #endregion

        #region Trigger callbacks
        private void OnTriggerEnter(Collider other)
        {
            if (player == null || player.state != Player.EPlayerState.Racing)
                return;

            if (other.CompareTag("FinishLine"))
            {
                player.EndRace(Time.timeSinceLevelLoad, Player.EPlayerState.FinishedCrossedTheLine);
            }
            else if (other.CompareTag("KillingFloor"))
            {
                player.EndRace(Time.timeSinceLevelLoad, Player.EPlayerState.FinishedKillingFloor);
            }
            else if (other.CompareTag("Checkpoint"))
            {
                if (other.GetComponent<Checkpoint>() != null) // verify that checkpoint component is indeed present, otherwise do nothing
                    player.CrossCheckpoint(other.GetComponent<Checkpoint>().order, Time.timeSinceLevelLoad);
            }          
        }
        #endregion
    }
}
