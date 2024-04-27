using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace TwitchConnect.Demo.GaltonTable
{
    public class GameManager : MonoBehaviour
    {
        public float startingPot = 8;
        public int maxTries = 25;

        public BoxCollider[] pits;
        public float[] probabilities;

        private BallSpawner spawner;

        [HideInInspector]
        public Trigger rollTrigger;
        [HideInInspector]
        public Action_ViewersPool pool;

        public bool activePlayer;
        public float currentPot;
        public float currentBet;
        public int currentUserID;
        public string currentUsername;
        public int pitSelected;
        public int numberOfTries;
        public bool gameInProgress;
        public float remainingTime;

        public List<LeaderboardEntry> leaderboard;

        private void Start()
        {
            rollTrigger = TwitchManager.instance.GetFeature("Roll + value").triggers[0];
            pool = TwitchManager.instance.GetFeature("Enter play queue").actions[0] as Action_ViewersPool;
            spawner = FindObjectOfType<BallSpawner>();
            currentUserID = -1;
            activePlayer = false;

            leaderboard = new List<LeaderboardEntry>();
        }

        private void Update()
        {
            if (currentUserID == -1) // waiting for pool to fill to pickup the next player
            {
                currentUserID = pool.DequeueNextViewer();
            }
            else if (!activePlayer)
            {
                // new challenger !
                Debug.Log("New game starting!");
                activePlayer = true;
                NewGame(currentUserID);
            }

            // timer for current roll. timeout after 45 seconds
            if (activePlayer)
                remainingTime -= Time.deltaTime;
            else
                remainingTime = 0;

            if (remainingTime <= 0 && activePlayer)
            {
                EndGame();
            }
        }

        public void NewGame(int userID)
        {
            currentPot = startingPot;
            currentUserID = userID;
            currentBet = 0;
            currentUsername = UserDictionnary.GetUserName(userID);
            numberOfTries = 0;
            gameInProgress = false;
            pitSelected = -1;
            rollTrigger.enabled = true; // open roll trigger
            remainingTime = 60f;
        }

        public void Roll(int userID, string message, object payload)
        {
            // checkif the commander is the current player
            if (userID != currentUserID)
                return;

            // parse pit from message
            pitSelected = int.Parse(message, CultureInfo.InvariantCulture);

            // check if it is a valid pit
            if (pitSelected < 1 || pitSelected > 11)
                return;

            StartCoroutine(StartNewRoll());
        }

        private IEnumerator StartNewRoll()
        {
            Debug.Log("New roll in progress.");

            spawner.DestroyBalls();

            rollTrigger.enabled = false;  // close trigger

            gameInProgress = true;
            currentPot -= 1;
            currentBet = (1 / probabilities[pitSelected - 1]) * 1.25f;
            numberOfTries++;
            remainingTime = 60f;

            // now proceed with the game
            spawner.BeginBallsGenerating();

            while (!spawner.ready)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            spawner.PickRandomBall();

            yield return new WaitForSeconds(0.3f);

            spawner.ReleaseBalls();
            spawner.OpenLatch();
        }

        public void Bank(int userID, string message, object payload)
        {
            // bank only when roll is available
            if (rollTrigger.enabled == false)
                return;

            if (userID != currentUserID)
                return;

            EndGame();
        }

        private void EndGame()
        {
            if (currentPot > 0)
            {
                // tally score and add to leaderboard if relevant
                LeaderboardEntry newEntry = new LeaderboardEntry(currentUsername, Mathf.Floor(currentPot));

                int insertIndex = -1;
                int currentIndex = 0;

                foreach (LeaderboardEntry lbe in leaderboard)
                {
                    if (currentPot > lbe.score)
                    {
                        insertIndex = currentIndex;
                        break;
                    }
                    currentIndex++;
                }

                if (insertIndex == -1)
                {
                    leaderboard.Add(newEntry); // add entry if no infoerior score found
                }
                else
                {
                    leaderboard.Insert(insertIndex, newEntry);
                }
            }
            
            currentUserID = -1;
            activePlayer = false;

            currentPot = 0;
            currentBet = 0;
            currentUserID = -1;
            currentUsername = "";
            numberOfTries = 0;
            gameInProgress = false;
            pitSelected = -1;
        }

        public void TriggerDetected(BoxCollider trigger)
        {
            int pitEntered = 0;
            gameInProgress = false;

            for (int i = 0; i < pits.Length; i++)
            {
                if (trigger == pits[i])
                {
                    pitEntered = i + 1;
                    Debug.Log("Pit entered : " + pitEntered);
                    break;
                }
            }

            // compare pit number with vote number
            if (pitEntered == pitSelected)
            {
                // succesful bet!
                currentPot += currentBet;
            }

            if (numberOfTries >= maxTries || currentPot < 1)
            {
                // GameEnds
                EndGame();
            }
            else
            {
                rollTrigger.enabled = true; // re-open roll command for next roll
            }
        }
    }
}