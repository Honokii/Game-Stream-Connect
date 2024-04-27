using UnityEngine;
using System.Collections.Generic;

namespace TwitchConnect.Demo.MarbleGame
{
    public class GameManager : MonoBehaviour
    {
        #region Public variables
        public static GameManager instance;


        [Header("General")]
        public int maxPlayers = 12;
        [SerializeField] private bool fillWithBots;
        [SerializeField] private int minimumPlayerNumberWithBots = 6;
        #endregion

        #region private variables
        private TwitchManager twitch;
        private PlayerManager playerManager;
        private Action_ViewersPool poolOfViewers;

        [Header("Game settings")]
        [SerializeField] private bool autoStartWhenReady;

        [SerializeField] private float playerFillDuration = 45f;
        [SerializeField] private float gameStartCountdownDuration = 3f;
        [SerializeField] private float maxGameDuration = 120f;
        [SerializeField] private float scoreboardDuration = 12f;

        [Header("Game state")]
        public EGameState state;

        [Header("Internal")]
        [SerializeField] private bool logs;
        [SerializeField] private float remainingFillTime;
        [SerializeField] private float countDownTimer;
        [SerializeField] public float elapsedGameProgressTime;
        [SerializeField] private float scoreboardElapsedTime;

        [HideInInspector] public List<Player> scoreboard;
        #endregion

        #region EGameState enum
        public enum EGameState
        {
            NotReady,
            Ready,
            PlayersFillingIn,
            GameStartCountdown,
            GameInProgress,
            Scoreboard
        }
        #endregion

        #region MB callbacks
        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(gameObject);    // Delete other next instance(s)

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            state = EGameState.NotReady;
        }

        // Update is called once per frame
        void Update()
        {
            if (!CheckContext())
            {
                state = EGameState.NotReady;
                return;
            }    

            switch (state)
            {
                case EGameState.NotReady:
                    if (twitch == null)
                        twitch = TwitchManager.instance;

                    if (playerManager == null)
                        playerManager = PlayerManager.instance;

                    if (twitch != null && playerManager != null)
                        NextState();
                    break;
                //============================================================
                case EGameState.Ready:
                    poolOfViewers.maxPoolSize = maxPlayers;
                    if (autoStartWhenReady)
                        NextState();
                    break;
                //============================================================
                case EGameState.PlayersFillingIn:
                    //if (players.players.Count > 0)
                    remainingFillTime -= Time.deltaTime;
                    AddNewPlayersIfAny();
                    if (remainingFillTime <= 0f || playerManager.players.Count == maxPlayers)
                        NextState();
                    break;
                //============================================================
                case EGameState.GameStartCountdown:
                    countDownTimer -= Time.deltaTime;
                    if (countDownTimer <= 0f)
                        NextState();
                    break;
                //============================================================
                case EGameState.GameInProgress:
                    elapsedGameProgressTime += Time.deltaTime;
                    //playerManager.players.Sort(new Player.SortByDescendingCheckpoint());
                    playerManager.players.Sort();
                    if (elapsedGameProgressTime > maxGameDuration || scoreboard.Count == playerManager.players.Count)
                        NextState();
                    break;
                //============================================================
                case EGameState.Scoreboard:
                    scoreboardElapsedTime += Time.deltaTime;

                    if (scoreboardElapsedTime > scoreboardDuration)
                        NextState();
                    break;
                //============================================================
                default:
                    break;
            }

            
        }

        #endregion

        #region Public methods
        public void NextState()
        {
            state = MoveNextState();
            if (logs) Debug.Log("Advanced to state " + state.ToString());
        }
        #endregion

        internal void PlayerFinished(Player p)
        {
            AddToScoreboard(p);
            if (p.state != Player.EPlayerState.FinishedCrossedTheLine)
                p.DestroyPlayersGameObject();
        }

        #region private methods

        private EGameState MoveNextState()
        {
            switch (state)
            {
                // manager have everything to proceed
                case EGameState.NotReady:
                    return EGameState.Ready;

                // beginning to instantiate players in pool queue AND open pool queue
                case EGameState.Ready:
                    poolOfViewers.opened = true;
                    playerManager.currentStartZoneBounds = (int)Mathf.Sqrt(maxPlayers) + 1;
                    remainingFillTime = playerFillDuration;
                    return EGameState.PlayersFillingIn;

                // game begins ; reopening pool and allowing even current players to enroll again for next session
                case EGameState.PlayersFillingIn:
                    poolOfViewers.ClearHistory();
                    countDownTimer = gameStartCountdownDuration;
                    if (fillWithBots)
                    {
                        while (playerManager.players.Count < minimumPlayerNumberWithBots)
                        {
                            playerManager.AddBot();
                        }
                    }
                    return EGameState.GameStartCountdown;

                // Starting countdown finished, releasing players
                case EGameState.GameStartCountdown:
                    playerManager.StartRace();
                    elapsedGameProgressTime = 0f;
                    scoreboard = new List<Player>();
                    return EGameState.GameInProgress;

                // game finished, tally scores etc.
                case EGameState.GameInProgress:
                    playerManager.EndRace();
                    scoreboardElapsedTime = 0f;
                    //scoreboard.Sort(new Player.SortByAscendingRaceTime());
                    scoreboard.Sort();
                    return EGameState.Scoreboard;

                // returning back to begining
                case EGameState.Scoreboard:
                    playerManager.ClearPlayers();
                    return EGameState.Ready;

                default:
                    return EGameState.NotReady;
            }
        }

        private void AddNewPlayersIfAny()
        {
            while (poolOfViewers.pool.Count > 0 && playerManager.players.Count < maxPlayers)
            {
                int userID = poolOfViewers.DequeueNextViewer();
                playerManager.AddPlayer(userID, UserDictionnary.GetUserName(userID));
            }

            playerManager.players.Sort();
        }

        private void AddToScoreboard(Player p)
        {
            if (scoreboard == null)
                scoreboard = new List<Player>();

            scoreboard.Add(p);
            scoreboard.Sort();
        }

        private bool CheckContext()
        {
            if (TwitchManager.instance == null)
            {
                Debug.LogWarning("No TwitchManager running in scene.");
                return false;
            }

            if (TwitchManager.instance.features == null)
            {
                Debug.LogWarning("No feature in TwitchManager, cannot look for viewers' pool.");
                return false;
            }

            if (poolOfViewers == null)
            {
                Feature f = TwitchManager.instance.GetFeature("Play");

                if (f == null)
                {
                    Debug.LogWarning("Cannot find Play feature in TwitchManager, cannot find viewer's pool.");
                    return false;
                }

                foreach (Action a in f.actions)
                {
                    if (a is Action_ViewersPool) // found it!
                    {
                        poolOfViewers = a as Action_ViewersPool;
                    }

                }
                if (poolOfViewers == null)
                {
                    Debug.LogWarning("Did find the feature in TwitchManager, but no pool found in action list. Cannot continue.");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region GUI
        private void OnGUI()
        {
            float boxOriginY = 215;
            float boxHeight;

            if (playerManager.players == null)
                playerManager.players = new List<Player>(); // NOT CLEAN AT ALL... this script is not responsible for the PM, but nobody sees it :D

            if (state != EGameState.Scoreboard)
                boxHeight = 55 + playerManager.players.Count * 20 + (playerManager.players.Count > 0 ? 5 : 0);
            else
                boxHeight = 55 + scoreboard.Count * 20 + (scoreboard.Count > 0 ? 5 : 0);

            string boxTitle = state.ToString();
            switch (state)
            {
                case EGameState.PlayersFillingIn:
                    boxTitle += " (" + (int)(remainingFillTime + 1) + ")";
                    break;
                case EGameState.GameStartCountdown:
                    boxTitle += " (" + (int)(countDownTimer + 1) + ")";
                    break;
                case EGameState.GameInProgress:
                    boxTitle += " (" + (int)(maxGameDuration - elapsedGameProgressTime + 1) + ")";
                    break;
                case EGameState.Scoreboard:
                    boxTitle += " (" + (int)(scoreboardDuration - scoreboardElapsedTime + 1) + ")";
                    break;
                default:
                    break;
            }

            // Make a background box
            GUI.Box(new Rect(10, boxOriginY, 180, boxHeight), boxTitle);

            if (GUI.Button(new Rect(20, boxOriginY + 25, 160, 20), "Next state"))
            {
                NextState();
            }

            float currentHeight = boxOriginY + 55;
            float stepHeight = 20;
            int currentPos = 1;

            if (state != EGameState.Scoreboard)
            {
                foreach (Player p in playerManager.players)
                {
                    if (p.state == Player.EPlayerState.Racing || p.state == Player.EPlayerState.Ready) // player is racing or about to
                    {
                        if (p.type == Player.EPlayerType.TwitchViewer)
                            GUI.color = Color.cyan;
                        else
                            GUI.color = Color.white;
                    }
                    else // player has finished
                    {
                        if (p.state == Player.EPlayerState.FinishedCrossedTheLine)
                            GUI.color = Color.green;
                        else
                        {
                            GUI.color = Color.gray;
                        }
                    }

                    GUI.Label(new Rect(20, currentHeight, 300, 20), currentPos + ". " + p.userName);
                    currentPos++;
                    currentHeight += stepHeight;
                }
            }
            else // scoreboard case
            {
                for (int i = 0; i < scoreboard.Count; i++)
                {
                    switch (scoreboard[i].state)
                    {
                        case Player.EPlayerState.FinishedCrossedTheLine:
                            GUI.color = i < 1 ? Color.yellow : GUI.color = Color.cyan;
                            GUI.Label(new Rect(20, currentHeight, 300, 20), currentPos + ". " + scoreboard[i].raceTime.ToString("F2") + " - " + scoreboard[i].userName);
                            break;
                        case Player.EPlayerState.FinishedRaceTimeOut:
                            GUI.color = Color.grey;
                            GUI.Label(new Rect(20, currentHeight, 300, 20), "DNF - " + scoreboard[i].userName);
                            break;
                        case Player.EPlayerState.FinishedKillingFloor:
                        default:
                            GUI.color = Color.grey;
                            GUI.Label(new Rect(20, currentHeight, 300, 20), "KIA - " + scoreboard[i].userName);
                            break;
                    }
                    currentPos++;
                    currentHeight += stepHeight;
                }
            }

        }
        #endregion
    }
}

