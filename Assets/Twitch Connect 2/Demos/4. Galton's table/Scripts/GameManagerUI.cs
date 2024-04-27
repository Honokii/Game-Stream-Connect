using UnityEngine;
using UnityEngine.UI;

namespace TwitchConnect.Demo.GaltonTable
{
    public class GameManagerUI : MonoBehaviour
    {
        public Text leaderboard;
        public Text queue;
        public Text stats;

        private GameManager game;

        private string current;

        // Start is called before the first frame update
        void Start()
        {
            game = FindObjectOfType<GameManager>();
        }

        // Update is called once per frame
        void Update()
        {
            current = "";

            foreach (LeaderboardEntry lbe in game.leaderboard)
            {
                current += lbe.time.ToString("T");
                current += " - $";
                current += lbe.score;
                current += " : ";
                current += lbe.userName;
                current += '\n';
            }

            leaderboard.text = current;

            current = "";

            if (game.pool.pool != null)
            {
                foreach (int userID in game.pool.pool)
                {
                    current += TwitchConnect.UserDictionnary.GetUserName(userID);
                    current += '\n';
                }

                queue.text = current;
            }

            current = "Current player : ";
            current += game.currentUsername;
            current += '\n';

            current += "Pot :      $";
            current += game.currentPot.ToString("N0");
            current += '\n';

            current += "Tries :   ";
            current += game.numberOfTries;
            current += '\n';

            current += "Time :   ";
            current += game.remainingTime.ToString("N1");
            current += " s";
            current += '\n';

            current += '\n';

            current += "Pit selected :    ";
            current += game.pitSelected;
            current += '\n';

            current += "Potential gain : $";
            current += game.currentBet.ToString("N0");

            stats.text = current;

        }
    }
}
