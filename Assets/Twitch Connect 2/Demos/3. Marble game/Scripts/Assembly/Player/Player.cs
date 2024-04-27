using System;

namespace TwitchConnect.Demo.MarbleGame
{
    [Serializable]
    public class Player : IComparable<Player>
    {
        // actual player gameObject
        public PlayerMonobehaviour mb;

        //context info for the player
        public int userId;
        public string userName;

        public float startTime;
        public float raceTime;
        public int lastCheckpoint;
        public float timeOfLastCheckpoint;

        public EPlayerState state;
        public EPlayerType type;

        public enum EPlayerState
        {
            FinishedCrossedTheLine = 0,
            Racing = 1,
            FinishedKillingFloor = 2,
            FinishedRaceTimeOut = 3,
            Ready = 4,
        }

        public enum EPlayerType
        {
            TwitchViewer,
            Bot,
            Ghost
        }

        public Player(PlayerMonobehaviour _mb, int _userId, string _userName, EPlayerType _type)
        {
            mb = _mb;
            userId = _userId;
            userName = _userName;
            type = _type;
        }

        #region public methods

        public void InitializePlayer()
        {
            state = EPlayerState.Ready;

            if (mb == null)
                return;
            mb.gameObject.name = "P: " + userName;
            mb.viewerId = userId;
            mb.viewerName = userName;
            mb.player = this;
            ContraintPhysics();
        }

        public void BeginRacing(float _startTime)
        {
            state = EPlayerState.Racing;
            startTime = _startTime;
            timeOfLastCheckpoint = _startTime;
            ReleasePhysics();
        }

        public void EndRace(float _finishTime, EPlayerState _finishType)
        {
            state = _finishType;
            raceTime = _finishTime - startTime;

            if (GameManager.instance == null)
                return;

            GameManager.instance.PlayerFinished(this);
        }

        public void DestroyPlayersGameObject()
        {
            if (mb == null)
                return;

            mb.DestroyGameObject();
        }

        public void CrossCheckpoint(int _checkpointIndex, float _time)
        {
            timeOfLastCheckpoint = _time;
            lastCheckpoint = _checkpointIndex;
        }

        #endregion

        #region private methods

        private void ReleasePhysics()
        {
            if (mb.rb == null)
                return;

            mb.rb.constraints = UnityEngine.RigidbodyConstraints.None;
        }

        private void ContraintPhysics()
        {
            if (mb.rb == null)
                return;

            mb.rb.constraints = UnityEngine.RigidbodyConstraints.FreezeAll;
        }
        #endregion

        #region IComparable
        public int CompareTo(Player other)
        {
            // compare enum states
            int result = (int)state - (int)other.state;

            // if not equal, simply order by enum
            if (result != 0)
                return result;
            else
            {
            // if equal, react accordingly to the state
                switch (state)
                {
                    case EPlayerState.FinishedCrossedTheLine:
                        if (raceTime > other.raceTime) return 1;
                        if (raceTime < other.raceTime) return -1;
                        return userId - other.userId;

                    case EPlayerState.Racing:
                        if (lastCheckpoint != other.lastCheckpoint)
                            return other.lastCheckpoint - lastCheckpoint;
                        else
                        {
                            if (timeOfLastCheckpoint > other.timeOfLastCheckpoint) return 1;
                            if (timeOfLastCheckpoint < other.timeOfLastCheckpoint) return -1;
                            return userId - other.userId;
                        }
                    default:
                        return userId - other.userId;
                }      
            }
        }
    }
    #endregion
}