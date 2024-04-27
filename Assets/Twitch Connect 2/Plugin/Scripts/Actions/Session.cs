namespace TwitchConnect
{
    /// <summary>
    /// Base class for all sessions. Derives from Action.
    /// </summary>
    public abstract class Session : Action
    {
        /// <summary>
        /// Allow viewers to create new options if vote option is not found ?
        /// <para>Usually votes don't allow for new options (you can't create a new vote as a viewer).</para>
        /// <para>Usually bets allow new options as they are based on numerical values.</para>
        /// </summary>
        public bool allowViewersToCreateNewOptions;

        internal Session() : base()
        {

        }

        internal Session(Session source) : base(source)
        {

        }

        #region general

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            // must be overriden to specific session behaviour
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// (Re)Open this session and forgets previous user history. Also used to restart a session (timer and history are reset);
        /// </summary>
        /// <param name="newDuration">Optionnally pass a different duration value.</param>
        public virtual void OpenSession(float newDuration = -1f)
        {
            opened = true;

            if (newDuration >= 0f)
                duration = newDuration;

            remainingTime = duration;

            ClearHistory();
        }

        public virtual void CloseSession()
        {
            opened = false;
            remainingTime = 0f;
            RefreshOptionsStatus();
        }

        public abstract void RefreshOptionsStatus();
        #endregion

    }
}