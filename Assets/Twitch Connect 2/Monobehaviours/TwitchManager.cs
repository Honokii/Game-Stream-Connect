using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using TwitchLib.Unity;
using TwitchLib.Client.Models;

namespace TwitchConnect
{
    /// <summary>
    /// Main component for TwitchConnect.
    /// </summary>
    [System.Serializable]
    public sealed class TwitchManager : MonoBehaviour
    {
        #region Public Variables

        public Client client;
        public Api api;

        [HideInInspector]
        /// <summary>
        /// Static instance of TwitchConnect object. This is used to access it from anywhere in your project.
        /// <para>If the Singleton pattern is activated, this is the static variable to be accessed between scenes.</para>
        /// </summary>
        public static TwitchManager instance;
        public static TwitchMessageBroadcaster messageBroadcaster;
        public static PubSub pubSub;
        public TwitchChannelInfos twitchChannelInfos;



        public List<Feature> features = new List<Feature>();
        public FeatureSet featureSet; // used as a ref to the scriptable object for loading/saving config.

        public Settings settings = new Settings();

        public TwitchLib.Api.Helix.Models.Users.GetUsers.User broadcaster;
        public Sprite streamerProfilePic;
        public Coroutine userConnection;
        public bool isDoneConnecting;
        public string botUsername;
        public string botOAuthToken;

        public TestCase testCase;
        public bool isInitialized { get { return client != null; } }

        public bool watchForViewers;
        public bool viewerWatcherCoroutineStarted;
        public event OnNewViewer OnNewViewer;
        public event OnViewerLeft OnViewerLeft;
        public event OnChannelRewardRedeem OnChannelRewardRedeem;

        #endregion

        #region Private Variables
        private bool connectionDesired;
        private float reconnectTimer;
        private const float reconnectEverySeconds = 10f;

        private Feature_HelpMessage f_helpFeature;
        private Feature_CommandList f_commandListFeature;
        private Feature_Chat f_chat;

        private bool streamInfosCoroutineStarted = false;
        private static HashSet<string> knownViewers;
        private static HashSet<string> workingSet1, workingSet2;
        #endregion

        #region Monobehaviour Callbacks

        private void Awake()
        {
            isDoneConnecting = false;

            InitializeSingleton();
            InitializeBroadcaster();
            InitializeChannelInfos();
            Secrets.LoadSecrets();
        }

        private void OnDisable()
        {
            Disconnect();
            DeinitializeFeatures();
        }

        void OnApplicationQuit()
        {
            instance = null;
        }

        private void Start()
        {
            if (settings.autoConnect)
            {
                Secrets.LoadSecrets();

                if (Secrets.last_Connection_Success > 0)
                {
                    userConnection = StartCoroutine(ConnectUsingKnownUserInfo());
                }
            }
        }

        private void Update()
        {
            CheckConnectionStatus();

            UpdateFeatures();

            if (settings.builtinGui && Input.GetKeyDown(settings.uIKeyCode))
                settings.showGUI = !settings.showGUI;
        }

        

        #endregion

        #region Public Methods

        /// <summary>
        /// return a feature referenced by its name. Returns null if not found. this reference should be cached for further access.
        /// </summary>
        /// <param name="_name">Name of the feature as string</param>
        /// <returns>Returns the first feature that matches name. Returns null if not found.</returns>
        public Feature GetFeature(string _name)
        {
            foreach (Feature f in features)
            {
                if (f.name.Equals(_name, System.StringComparison.InvariantCulture))
                    return f;
            }

            return null;
        }

        public Feature GetFeature(Trigger trigger)
        {
            if (features == null) return null;

            foreach (Feature f in features)
            {
                foreach (Trigger t in f.triggers)
                {
                    if (t == trigger)
                        return f;
                }
            }

            return null;
        }

        public Feature GetFeature(Action action)
        {
            if (features == null) return null;

            foreach (Feature f in features)
            {
                foreach (Action a in f.actions)
                {
                    if (a == action)
                        return f;
                }
            }

            return null;
        }

        public void ImportFeatureSet(FeatureSet set)
        {
/*            if (set == null)
                return;

            features = new List<Feature>();

            foreach (Feature f in set.features)
            {
                LoadFeature(f);
            }

            featureSet = set;*/
        }

        public void ImportFeatureSet()
        {
/*            if (featureSet == null)
            {
                Debug.LogWarning("Cannot load feature set: no feature set selected.");
                return;
            }

            foreach (Feature f in featureSet.features)
            {
                LoadFeature(f);
            }*/
        }
#if UNITY_EDITOR
        public void ExportFeatureSet()
        {
            if (features.Count == 0)
                return;

            if (featureSet == null)
            {
                ExportFeatureSetAsNew();
            }

            ExportFeatureSet(features);
        }

        public void ExportFeatureSetAsNew()
        {
            if (features.Count == 0)
                return;

            if (features == null)
                return;

            ExportFeatureSetAsNew(features);
        }
#endif
        public void WriteToChat(string message)
        {
            client.SendMessage(client.JoinedChannels[0], message);
        }

        public void Connect()
        {
            if (!isInitialized)
                Initialize();
            else if (client.IsConnected)
                return;

            if (pubSub == null)
                InitializePubSub();

            client.Connect();
            pubSub.Connect();
        }

        public void Initialize()
        {
            InitializeClient();
            InitializeApi();
            UserDictionnary.Initialize();
            InitializeFeatures();
            client.OnSendReceiveData += Client_OnSendReceiveData;

#if UNITY_EDITOR
            InitializeTester(); 
#endif
        }

        public void Disconnect()
        {
            connectionDesired = false;

            if (client != null && client.IsConnected)
                client.Disconnect();

            if (pubSub != null)
                pubSub.Disconnect();
        }

        #endregion

        #region internal methods

        internal void EnableTwitchChatFeature()
        {
            settings.broadcastTwitchChat = true;
        }

        internal void SendHelpMessage(string username)
        {
            if (settings.helpMessage.Length == 0)
                return;

            WriteToChat(username + " " + settings.helpMessage);
        }

        internal void SendCommandList(string username)
        {
            if (client == null)
                return;

            string globalCommandList = "";

            if (settings.helpCommand && settings.helpMessage.Length > 0)
                globalCommandList += "help";

            foreach (Feature f in features)
            {
                if (f.GetCommandList().Length > 0)
                {
                    if (globalCommandList.Length > 0)
                        globalCommandList += ", ";

                    globalCommandList += f.GetCommandList();
                }

            }

            if (globalCommandList.Length > 0)
            {
                globalCommandList = " List of commands: " + globalCommandList;
                globalCommandList = username + globalCommandList;
            }
            else
            {
                globalCommandList = username + " No active command ATM.";
            }

            WriteToChat(globalCommandList);
        }

        internal void StartCoroutineGetChannelInfo()
        {
            if (!streamInfosCoroutineStarted)
                StartCoroutine("GetChannelInfos");
        }

        #endregion

        #region Private Methods

#if UNITY_EDITOR
        private void CleanAssetDatabase()
        {
            AssetDatabase.Refresh();

            // remove all unused SOs from project library
            List<Trigger> allTriggers = FindAssetsByType<Trigger>();
            List<Action> allActions = FindAssetsByType<Action>();
            List<FeatureSet> allFeatureSets = FindAssetsByType<FeatureSet>();

            foreach (FeatureSet fs in allFeatureSets)
            {
                foreach (Feature f in fs.features)
                {
                    foreach (Trigger t in f.triggers)
                    {
                        if (allTriggers.Contains(t))
                            allTriggers.Remove(t);
                    }

                    foreach (Action a in f.actions)
                    {
                        if (allActions.Contains(a))
                            allActions.Remove(a);
                    }
                }
            }

            foreach (Trigger t in allTriggers)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(t));
            }

            foreach (Action a in allActions)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(a));
            }

            AssetDatabase.SaveAssets();
        }
#endif
        private void LoadFeature(Feature f)
        {
/*            if (features == null)
                features = new List<Feature>();

            if (!features.Contains(f))
                features.Add(new Feature(f));

            if (Application.isPlaying)
                f.Initialize();*/
        }

        private void InitializeTester()
        {
            if (testCase == null)
                testCase = ScriptableObject.CreateInstance("TestCase") as TestCase;

            testCase.Initialize(this);
        }

        private void InitializeBroadcaster()
        {
            if (messageBroadcaster == null)
                messageBroadcaster = new TwitchMessageBroadcaster();
        }

        private void InitializeChannelInfos()
        {
            twitchChannelInfos = new TwitchChannelInfos();
            knownViewers = new HashSet<string>();
            workingSet1 = new HashSet<string>();
            workingSet2 = new HashSet<string>();
        }

        private void UpdateFeatures()
        {
            if (client == null)
                return;

            foreach (Feature f in features)
            {
                f.Update();
            }

            if (settings.helpCommand)
            {
                if (f_helpFeature == null)
                    f_helpFeature = new Feature_HelpMessage();

                f_helpFeature.Update();
            }

            if (settings.listCommand)
            {
                if (f_commandListFeature == null)
                    f_commandListFeature = new Feature_CommandList();

                f_commandListFeature.Update();
            }

            if (settings.broadcastTwitchChat)
            {
                if (f_chat == null)
                    f_chat = new Feature_Chat();

                f_chat.Update();
            }
        }

        private void DeinitializeFeatures()
        {
            foreach (Feature f in features)
            {
                f.Deinitialize();
            }

            if (f_helpFeature != null)
                f_helpFeature.Deinitialize();

            if (f_commandListFeature != null)
                f_commandListFeature.Deinitialize();

            if (f_chat != null)
                f_chat.Deinitialize();
        }

        private void InitializeFeatures()
        {
            foreach (Feature f in features)
            {
                f.Initialize();
            }
        }

        private void InitializeSingleton()
        {
            if (settings.singleton)
            {
                if (instance != null && instance != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    instance = this;
                }

                DontDestroyOnLoad(this);
            }
            else
            {
                instance = this;
            }
        }

        private void InitializePubSub()
        {
#pragma warning disable CS1701 // Assuming assembly reference matches identity
            pubSub = new PubSub();
#pragma warning restore CS1701 // Assuming assembly reference matches identity

            pubSub.OnPubSubServiceConnected += PubSub_OnPubSubServiceConnected;
            pubSub.OnPubSubServiceClosed += PubSub_OnPubSubServiceClosed;
            pubSub.OnPubSubServiceError += PubSub_OnPubSubServiceError;
            pubSub.OnLog += PubSub_OnLog;

            pubSub.OnListenResponse += PubSub_OnListenResponse;

#pragma warning disable CS0618 // Type or member is obsolete
            pubSub.OnRewardRedeemed += PubSub_OnChannelRewardRedeemed;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private void PubSub_OnLog(object sender, TwitchLib.PubSub.Events.OnLogArgs e)
        {
            if (settings.verbose)
                Debug.Log($"[Twitch Manager] [PubSub] {e.Data}");
        }

        private void PubSub_OnListenResponse(object sender, TwitchLib.PubSub.Events.OnListenResponseArgs e)
        {
            if (!settings.verbose)
                return;

            if (e.Successful)
            {
                Debug.Log($"[Twitch Manager] Successfully verified listening to topic: {e.Topic}");
            }
            else
            {
                Debug.Log($"[Twitch Manager] Failed to listen! Error: {e.Response.Error}");
            }
        }

        private void PubSub_OnPubSubServiceError(object sender, TwitchLib.PubSub.Events.OnPubSubServiceErrorArgs e)
        {
            if (settings.verbose)
                Debug.Log("[Twitch Manager] PubSub Service Connection error : " + e.Exception.Message);
        }

        private void PubSub_OnPubSubServiceClosed(object sender, System.EventArgs e)
        {
            if (settings.verbose)
                Debug.Log("[Twitch Manager] PubSub service disconnected");
        }

        private void PubSub_OnPubSubServiceConnected(object sender, System.EventArgs e)
        {
            if (settings.verbose)
                Debug.Log("[Twitch Manager] PubSub Service Connected!");

            //pubSub.ListenToChannelPoints(Secrets.channel_id);
#pragma warning disable CS0618 // Le type ou le membre est obsolète
            pubSub.ListenToRewards(Secrets.channel_id);
#pragma warning restore CS0618 // Le type ou le membre est obsolète
            pubSub.ListenToFollows(Secrets.channel_id);

            // SendTopics accepts an oauth optionally, which is necessary for some topics, such as bit events or custom rewards.
            pubSub.SendTopics(Secrets.game_oauth_token);
        }

        private void PubSub_OnChannelRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs e)
        {
            OnChannelRewardRedeem?.Invoke(sender, e);
        }

        private void Client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            Debug.Log("[Twitch Manager] Connected!");
            if (settings.useBot)
                Debug.Log("[Twitch Manager] Game will engage in chat as bot " + client.TwitchUsername);
            StartCoroutineGetChannelInfo();
            Secrets.SetConnectionSuccess(true);
            Secrets.SaveSecrets(); // update secrets in playerprefs on succesful connection (param = 1)

            connectionDesired = true; // keep client connected if it gets disconnected.
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            Debug.Log("[Twitch Manager] Disconnected.");
        }

        private void Client_OnSendReceiveData(object sender, TwitchLib.Client.Events.OnSendReceiveDataArgs e)
        {
            if (settings.verbose)
                Debug.Log(e.Data);

            if (e.Data.Contains("ROOMSTATE") && e.Data.Contains("room-id="))
            {
                int begPos = e.Data.IndexOf("room-id=", System.StringComparison.InvariantCulture) + 8;
                string channel_id = e.Data.Substring(begPos);
                int endPos = channel_id.IndexOf(";", System.StringComparison.InvariantCulture);
                channel_id = channel_id.Substring(0, endPos);

                foreach (char c in channel_id)
                {
                    if (c < '0' || c > '9')
                    {
                        //Debug.Log("bad channel id : " + channel_id);
                        return;
                    }

                }
                Secrets.channel_id = channel_id;
            }
        }

        private void CheckConnectionStatus()
        {
            if (connectionDesired && client != null && !client.IsConnected)
            {
                reconnectTimer -= Time.deltaTime;

                if (reconnectTimer <= 0f)
                {
                    reconnectTimer = reconnectEverySeconds;
                    Reconnect();
                }
                               
            }
        }

        private void Reconnect()
        {
            Debug.Log("[Twitch Manager] Unexpectedly disconnected. Trying to reconnect.");

            // reset sensitive stuff
            client = null;
            pubSub = null;

            InitializeClient(); // this will prevent actions to be reinitialized (thus losing user history and countdowns etc.)
            InitializeBroadcaster();
            
            client.OnSendReceiveData += Client_OnSendReceiveData; // IKR...!

            Connect();

            foreach (Feature f in features)
            {
                f.ReInitialize();
            }
            UserDictionnary.ReInitialize();
            f_helpFeature?.ReInitialize();
            f_commandListFeature?.ReInitialize();
            f_chat?.ReInitialize();
        }

        private void InitializeClient()
        {
            Application.runInBackground = true;
            client = new Client();

            if (settings.useBot) // if a bot is needed, we need to pass credentials to the static secret class
            {
                Secrets.IRC_bot_name = botUsername;
                Secrets.IRC_bot_oauth = botOAuthToken;
            }

            ConnectionCredentials credentials = new ConnectionCredentials(
                settings.useBot ? Secrets.IRC_bot_name : Secrets.channel_name,
                settings.useBot ? Secrets.IRC_bot_oauth : Secrets.game_oauth_token);

            client.Initialize(credentials, Secrets.channel_name);

            // event subscription starts here
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;

            // also add command if not the default one:
            if (settings.commandPrefix.ToString() != "" && settings.commandPrefix != '!')
            {
                client.RemoveChatCommandIdentifier('!');
                client.AddChatCommandIdentifier(settings.commandPrefix);
            }
        }

        private void InitializeApi()
        {
            Application.runInBackground = true;
            api = new Api();
            api.Settings.ClientId = Secrets.game_client_id; // APP ID
            api.Settings.AccessToken = Secrets.game_oauth_token; // USER TOKEN APP (linked to APP ID)
        }

#if UNITY_EDITOR
        private void ExportFeatureSetAsNew(List<Feature> _features)
        {
            if (_features == null)
                return;

            FeatureSet asset = ScriptableObject.CreateInstance<FeatureSet>();

            string featureName = "";

            foreach (Feature f in _features)
            {
                asset.features.Add(new Feature(f));
                featureName += featureName.Length > 0 ? ", " + f.name : f.name;
            }

            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Twitch Connect 2/Scriptable Objects/Features/" + featureName + ".asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            //EditorUtility.FocusProjectWindow();
            //Selection.activeObject = asset;

            // keep the ref to the newly created object
            featureSet = asset;
        }

        private void ExportFeatureSet(List<Feature> _features)
        {
            if (_features == null)
                return;

            // we simply update the current ref'd featureSet with current asset values.
            if (featureSet == null)
                return;

            featureSet.features = new List<Feature>();

            // copy features to asset
            foreach (Feature f in _features)
            {
                featureSet.features.Add(new Feature(f));
            }

            CleanAssetDatabase();
        }

        public static List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }
#endif
        #endregion

        #region Coroutines

        internal IEnumerator UpdateKnownViewers()
        {
            viewerWatcherCoroutineStarted = true;                

            if (knownViewers == null)
                knownViewers = new HashSet<string>();
            if (workingSet1 == null)
                workingSet1 = new HashSet<string>();
            if (workingSet2 == null)
                workingSet2 = new HashSet<string>();

            while (client.IsConnected && watchForViewers)
            {
                if (twitchChannelInfos == null || twitchChannelInfos.chatters == null || twitchChannelInfos.chatters.allViewers == null)
                {
                    viewerWatcherCoroutineStarted = false;
                    yield break;
                }

                workingSet1.Clear();
                foreach (string s in twitchChannelInfos.chatters.allViewers)
                {
                    workingSet1.Add(s);
                }

                // NEW viewer aka viewers in chat but unknown
                workingSet1.ExceptWith(knownViewers);
                if (workingSet1.Count > 0)
                {
                    HashSet<int> newViewersList = new HashSet<int>();
                    // remove those for which we already know the userID
                    HashSet<string> toRemove1 = new HashSet<string>();
                    foreach (string s in workingSet1)
                    {
                        
                        int userID = UserDictionnary.GetUserIDFromUsername(s);
                        if (userID > 0)
                        {
                            knownViewers.Add(s);
                            newViewersList.Add(userID);
                            toRemove1.Add(s);
                        }
                    }
                    workingSet1.ExceptWith(toRemove1);

                    // now remains only the users that we don't know yet.
                    // Ask twitch about them, and add them to dictionnary
                    List<string> workingStrings1 = new List<string>(workingSet1);

                    TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse getUsersResponse = null;
                    yield return api.InvokeAsync(
                        api.Helix.Users.GetUsersAsync(logins:workingStrings1),
                        (response) => getUsersResponse = response);
                    // ===============================================
                    // we will reach this point only when async request is done
                    foreach (var u in getUsersResponse.Users)
                    {
                        int userID = int.Parse(u.Id);
                        UserDictionnary.AddToDictionnary(userID, u.DisplayName);
                        newViewersList.Add(userID);
                    }
                    // DONE ! in variable newViewersList

                    foreach (int userID in newViewersList)
                    {
                        OnNewViewer?.Invoke(userID);
                    }
                }

                // now do the same for viewers that left.
                // supposedly we only need to trim current viewers from known viewers

                // NEW viewer : viewers in chat but unknown
                workingSet2.Clear();
                workingSet2.UnionWith(knownViewers);
                workingSet2.ExceptWith(twitchChannelInfos.chatters.allViewers); // remains the known viewers that are not in chat
                if (workingSet2.Count > 0)
                {
                    HashSet<int> leftViewersList = new HashSet<int>();
                    // retrieve the userIDs
                    foreach (string s in workingSet2)
                    {
                        int userID = UserDictionnary.GetUserIDFromUsername(s);
                        if (userID > 0)
                        {
                            OnViewerLeft?.Invoke(userID);
                            //Debug.Log("[Twitch Manager]New viewer : " + userID + " name : " + UserDictionnary.GetUserName(userID));
                        }
                    }
                }

                knownViewers.Clear();
                knownViewers.UnionWith(twitchChannelInfos.chatters.allViewers); // reset the known viewer hashset until next time

                yield return new WaitForSeconds(20);
            }

            viewerWatcherCoroutineStarted = false; // inform MB that we stopped this
        }

        internal IEnumerator GetChannelInfos()
        {
            // to avoid double starts of coroutines
            streamInfosCoroutineStarted = true;

            twitchChannelInfos = new TwitchChannelInfos();
            string jsonText;

            while (client.IsConnected)
            {
                UnityWebRequest www = UnityWebRequest.Get("https://tmi.twitch.tv/group/user/" + Secrets.channel_name + "/chatters");

                yield return www.SendWebRequest();

#if UNITY_2020 || UNITY_2020_3_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
#else   
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
#endif
                else
                {
                    // Get results as text
                    jsonText = www.downloadHandler.text;

                    // We don't need the UWR anymore
                    www.Dispose();

                    // parse the retrieved JSON object to our custom object
                    twitchChannelInfos = JsonUtility.FromJson<TwitchChannelInfos>(jsonText);
                    twitchChannelInfos.chatters.Update();
                }

                yield return new WaitForSeconds(20);
            }

            // Coroutine finishing, exposing that info
            streamInfosCoroutineStarted = false;
        }

        internal IEnumerator ConnectUsingKnownUserInfo()
        {
            int userID;
            string userName;

            UnityWebRequest www = UnityWebRequest.Get("https://id.twitch.tv/oauth2/validate");
            www.SetRequestHeader("Authorization", "Bearer " + Secrets.game_oauth_token);
            yield return www.SendWebRequest();

#if UNITY_2020 || UNITY_2020_3_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                if (settings.verbose)
                    Debug.Log(www.error);
                www.Dispose();

                // try to reffresh the token
                if (settings.verbose)
                    Debug.Log("[Twitch Manager] Refreshing access token from Twitch");
                // HERE

                WWWForm form = new WWWForm();
                form.AddField("grant_type", "refresh_token");
                form.AddField("refresh_token", Secrets.game_oauth_refresh_token);
                form.AddField("client_id", Secrets.game_client_id);
                form.AddField("client_secret", Secrets.game_client_secret);

                www = UnityWebRequest.Post("https://id.twitch.tv/oauth2/token", form);
                yield return www.SendWebRequest();

#if UNITY_2020 || UNITY_2020_3_OR_NEWER
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
                {
                    if (settings.verbose)
                    {
                        Debug.Log(www.error);
                        Debug.Log("[Twitch Manager] Error while refreshing access token. You might need to redo the OAuth process.");
                    }   
                    yield break;
                }
                else
                {
                    if (settings.verbose)
                        Debug.Log("[Twitch Manager] Access token succesfully refreshed.");

                    // SUCCESS, update new
                    // Get results as text
                    string tokenResult = www.downloadHandler.text;

                    // We don't need the UWR anymore
                    www.Dispose();

                    TokenRefreshResponse tokenResponse = new TokenRefreshResponse();

                    // parse the retrieved JSON object to our custom object
                    tokenResponse = JsonUtility.FromJson<TokenRefreshResponse>(tokenResult);

                    Secrets.game_oauth_token = tokenResponse.access_token;
                    Secrets.game_oauth_refresh_token = tokenResponse.refresh_token;

                    // Now we can retry the request : 
                    www = UnityWebRequest.Get("https://id.twitch.tv/oauth2/validate");
                    www.SetRequestHeader("Authorization", "Bearer " + Secrets.game_oauth_token);
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                    {
                        // there is nothing we can do..!
                        Debug.Log(www.error);
                        yield break;
                    }
                }
            }

            // Get results as text
            string jsonText = www.downloadHandler.text;

            // We don't need the UWR anymore
            www.Dispose();

            UserValidateResponse response = new UserValidateResponse();

            // parse the retrieved JSON object to our custom object
            response = JsonUtility.FromJson<UserValidateResponse>(jsonText);

            userID = int.Parse(response.user_id);
            userName = response.login;

            Api api = new Api();

            api.Settings.AccessToken = Secrets.game_oauth_token;
            api.Settings.ClientId = Secrets.game_client_id;

            TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse getUsersResponse = null;

            yield return api.InvokeAsync(
                api.Helix.Users.GetUsersAsync(ids: new List<string> { userID.ToString() }),
                (response2) => getUsersResponse = response2);
            // ===============================================
            // we will reach this point only when async request is done

            if (getUsersResponse.Users.Length > 0)
            {
                broadcaster = getUsersResponse.Users[0];
            }
            else
            {
                yield break;
            }

            www = UnityWebRequestTexture.GetTexture(broadcaster.ProfileImageUrl);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                streamerProfilePic = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            }

            Secrets.channel_name = broadcaster.Login;
            Secrets.channel_id = broadcaster.Id;
            Secrets.SetConnectionSuccess(true);
            Secrets.SaveSecrets();
            Connect();
        }

        #endregion

        #region GUI

        void OnGUI()
        {
           if (!settings.builtinGui || !settings.showGUI)
            {
                return; // return if GUI not active or if we are not an Admin
            }

            DrawGUI();
        }

        private void DrawGUI()
        {
            float boxOriginX = 10;
            float boxOriginY = 10;
            float boxHeight = 190;
            float boxWidth = 250;
            float defaultOuterMargin = 10;
            float defaultContentMargin = 90;
            float defaultContentHeight = 23;

            float leftLabelsAlignment = boxOriginX + defaultOuterMargin;
            float leftContentAlignment = boxOriginX + defaultContentMargin;
            float topAlignment = boxOriginY + defaultOuterMargin;
            float labelWidth = defaultContentMargin - defaultOuterMargin;
            float contentWidth = Mathf.Max(0f, boxWidth - defaultOuterMargin - defaultOuterMargin - labelWidth);

            float currentYPosition = boxOriginY;

            // Background box
            GUI.Box(new Rect(boxOriginX, boxOriginY, boxWidth, boxHeight), "Twitch Connect");

            // title only
            currentYPosition += defaultContentHeight;

            // App id
            GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "App ID");
            GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), Secrets.game_client_id);

            currentYPosition += defaultContentHeight;

            // Connect Button
            if (Secrets.last_Connection_Success < 1)
                GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, boxWidth - (2* defaultOuterMargin), defaultContentHeight), "No valid credentials, please run Implicit OAuth first.");
            else
            {
                if (client == null || !client.IsConnected)
                {
                    if (userConnection == null && GUI.Button(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), "Connect"))
                        // Clicking on CONNECT button
                        userConnection = StartCoroutine(ConnectUsingKnownUserInfo());
                    else if (userConnection != null)
                        // userConnection is already running, display wait message
                        GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), "Connecting...");
                }
                else if (client.IsConnected)
                    // client is connected
                    currentYPosition -= defaultContentHeight; // dont show this line
                else
                    // other case
                    GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), "N/A");
            }

            currentYPosition += defaultContentHeight;

            if (client != null && client.IsConnected)
            {
                // username
                GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "Channel");
                GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), Secrets.channel_name);
                currentYPosition += defaultContentHeight;
                // userID
                GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "Channel ID");
                GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), Secrets.channel_id);
                currentYPosition += defaultContentHeight;
                // Profil Pic
                GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "Profile Pic");
                GUIDrawSprite(new Rect(leftContentAlignment, currentYPosition, defaultContentHeight, defaultContentHeight), streamerProfilePic);
                currentYPosition += defaultContentHeight;
                // Total channel viewer count
                GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "Total views");
                GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), broadcaster.ViewCount.ToString());
                currentYPosition += defaultContentHeight;
                // Live viewers
                GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "Live views");
                if (twitchChannelInfos.chatter_count == 0)
                {
                    GUI.contentColor = Color.red;
                    GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), "0 (something's wrong...)");
                }
                else
                {
                    GUI.contentColor = Color.green;
                    GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), twitchChannelInfos.chatter_count.ToString());
                }
                GUI.contentColor = Color.white;
                currentYPosition += defaultContentHeight;
                // Bot name if applicable
                if (settings.useBot)
                {
                    GUI.Label(new Rect(leftLabelsAlignment, currentYPosition, defaultContentMargin, defaultContentHeight), "Bot name");
                    GUI.Label(new Rect(leftContentAlignment, currentYPosition, contentWidth, defaultContentHeight), Secrets.IRC_bot_name.ToString());
                    currentYPosition += defaultContentHeight;
                }
            }
        }

        public static void GUIDrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null)
                return;
            Rect spriteRect = sprite.rect;
            Texture2D tex = sprite.texture;
            GUI.DrawTextureWithTexCoords(rect, tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));
        }

        #endregion
    }
}