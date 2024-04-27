using UnityEngine;

namespace TwitchConnect.Demo.GaltonTable
{
    public class BallSpawner : MonoBehaviour
    {
        public bool begin = false;
        public bool release = false;

        public GameObject ballPrefab; // spheres only
        public Material playerBallMaterial;

        public int width;

        private float currentX, currentY;
        private int currentIndex;

        public int quantity;
        private float ballSize;
        public bool generating;
        public bool ready = false;

        private GameObject[] balls;
        private Rigidbody[] rigidbodies;

        public GameObject latch;
        private Vector3 lockedPosition;

        // Start is called before the first frame update
        void Start()
        {
            generating = false;
            ready = false;
            lockedPosition = latch.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (begin)
            {
                begin = false;
                BeginBallsGenerating();
            }

            if (release)
            {
                release = false;
                ReleaseBalls();
            }

            if (generating)
            {
                AddNewBall();
            }
        }

        private void AddNewBall()
        {
            if (balls == null)
                balls = new GameObject[quantity];

            if (rigidbodies == null)
                rigidbodies = new Rigidbody[quantity];

            if (currentIndex >= quantity) // proceed only if quantity is not reach yet ; set ready otherwise
            {
                generating = false;
                ready = true;
                return;
            }

            balls[currentIndex] = Instantiate(ballPrefab, new Vector3(currentX, currentY, transform.position.z), Quaternion.identity);
            rigidbodies[currentIndex] = balls[currentIndex].GetComponent<Rigidbody>();
            rigidbodies[currentIndex].isKinematic = true; // fix balls at first

            currentIndex++;

            currentX += ballSize; // for the next time, try to spawn directly on the right

            if (currentX >= width / 2) // if too far, go back on left and go to next layer
            {
                currentY += ballSize;
                currentX = - width / 2;
            }
        }

        public void BeginBallsGenerating()
        {
            ballSize = ballPrefab.transform.localScale.x;
            balls = new GameObject[quantity];
            generating = true;
            currentIndex = 0;

            currentX = - width / 2;
            currentY = transform.position.y;

            CloseLatch();
        }

        public void PickRandomBall()
        {
            if (!ready || generating)
                return;

            int selectedBallIndex = Random.Range(0, quantity);

            GameObject selectedBall = balls[selectedBallIndex];
            selectedBall.GetComponent<MeshRenderer>().material = playerBallMaterial;
            selectedBall.AddComponent<BallTrigger>(); // for the ball to check for pits triggers
        }

        public void DestroyBalls()
        {
            if (balls != null)
            {
                foreach (GameObject go in balls)
                {
                    Destroy(go);
                }
            }

            currentIndex = 0;
            generating = false;
            ready = false;
        }

        public void EnableBallsPhysics()
        {
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
            }
        }

        public void ReleaseBalls()
        {
            generating = false;
            ready = false;
            EnableBallsPhysics();
        }

        public void OpenLatch()
        {
            latch.transform.Translate(-Vector3.forward * latch.transform.localScale.z); // move the latch away
        }

        public void CloseLatch()
        {
            latch.transform.SetPositionAndRotation(lockedPosition, latch.transform.rotation);
        }
    }
}
