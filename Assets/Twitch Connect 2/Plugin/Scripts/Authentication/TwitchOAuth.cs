using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TwitchLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace TwitchConnect
{
    /// <summary>
    /// This script expects a TwtichManager component, and will auto connect it & fill-in secrets on its behalf.
    /// </summary>
    [RequireComponent(typeof(TwitchApiCallHelper))]
    public class TwitchOAuth : MonoBehaviour
    {
        public bool isIdentified;
        public bool isRunning;
        public bool isDoneSuccess;

        [Header("Authentication")]
        [SerializeField] private string twitchAuthUrl = "https://id.twitch.tv/oauth2/authorize";
        [SerializeField] private string twitchClientId = "PUT YOUR CLIENT ID HERE";
        [SerializeField] private string twitchClientSecret = "PUT YOUR CLIENT SECRET HERE";
        [SerializeField] private string twitchRedirectUrl = "http://localhost:8080/";

        [Header("Broadcaster")]
        public Sprite _profilePicture;
        public string _username;
        public int _userId;
        public string _authToken;
        public string _refreshToken;
        private TwitchLib.Api.Helix.Models.Users.GetUsers.User broadcaster;

        [Header("Optional UI fields")]
        [SerializeField] private Text uiTextToken = null;
        [SerializeField] private Text uiTextusername = null;
        [SerializeField] private Text uiTextuserId = null;
        [SerializeField] private Text uiTextViewcount = null;
        [SerializeField] private Text uiTextProfilPicUrl = null;
        [SerializeField] private Text uiTextemail = null;
        [SerializeField] private Image uiImageProfilePicture = null;
        [SerializeField] private Text uiTextProfileName = null;
        [SerializeField] private GameObject mainButton = null;
        [SerializeField] private GameObject runningPanel = null;
        [SerializeField] private GameObject mainTitle = null;
        [SerializeField] private GameObject mainTitlePrompt = null;
        [SerializeField] private GameObject credentialSavedPrompt = null;

        private TwitchApiCallHelper twitchApiCallHelper;
        private string _twitchAuthStateVerify;

        private Coroutine ValidateUserId;

        public void Start()
        {
            _authToken = "";
            _refreshToken = "";
            _userId = -1;
            _username = "";

            broadcaster = new TwitchLib.Api.Helix.Models.Users.GetUsers.User();

            isDoneSuccess = false;
            isIdentified = false;

            Secrets.LoadSecrets();

            if ((twitchClientId == null || twitchClientId.Length == 0 || twitchClientId == "PUT YOUR CLIENT ID HERE")
                && (twitchClientSecret == null || twitchClientSecret.Length == 0 || twitchClientSecret == "PUT YOUR CLIENT SECRET HERE")
                && Secrets.last_Connection_Success > 0
                && Secrets.game_client_id.Length > 0 
                && Secrets.game_client_secret.Length > 0)
            {
                twitchClientId = Secrets.game_client_id;
                twitchClientSecret = Secrets.game_client_secret;

                Debug.Log("[Twitch OAuth] Missing client id and secret but found some in PlayerPrefs. Using them instead.");
            }

            twitchApiCallHelper = GetComponent<TwitchApiCallHelper>();
            UpdateDisplay();
            StartCoroutine(DisplayUpdater());

            if (TwitchManager.instance != null)
            {
                // take over control of twitch manager connection
                TwitchManager.instance.settings.autoConnect = false;
            }
        }

        private void Update()
        {
            if (_authToken != null && _authToken.Length > 0 && ValidateUserId == null)
                ValidateUserId = StartCoroutine(ValidateUserInfo());

        }

        private void LoadImage(Texture2D texture)
        {
            _profilePicture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            uiImageProfilePicture.color = new Color(1, 1, 1, 1); //show image
        }

        /// <summary>
        /// Starts the Twitch OAuth flow by constructing the Twitch auth URL based on the scopes you want/need.
        /// </summary>
        public void Connect()
        {
            isIdentified = false;
            isRunning = true;

            string[] scopes;
            string s;

            // list of scopes we want
            scopes = new[]
            {
            "user:read:email",
            "chat:edit",
            "chat:read",
            "channel:read:redemptions",
            "channel:read:subscriptions",
            "user:read:broadcast",
            "channel:manage:redemptions"
        };

            // generate something for the "state" parameter.
            // this can be whatever you want it to be, it's gonna be "echoed back" to us as is and should be used to
            // verify the redirect back from Twitch is valid.
            _twitchAuthStateVerify = ((Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

            // query parameters for the Twitch auth URL
            s = "client_id=" + twitchClientId + "&" +
                "redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl) + "&" +
                "state=" + _twitchAuthStateVerify + "&" +
                "response_type=code&" +
                "scope=" + String.Join("+", scopes);

            // start our local webserver to receive the redirect back after Twitch authenticated
            StartLocalWebserver();

            // open the users browser and send them to the Twitch auth URL
            Application.OpenURL(twitchAuthUrl + "?" + s);
        }

        /// <summary>
        /// Opens a simple "webserver" like thing on localhost:8080 for the auth redirect to land on.
        /// Based on the C# HttpListener docs: https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener
        /// </summary>
        private void StartLocalWebserver()
        {
            HttpListener httpListener = new HttpListener();

            httpListener.Prefixes.Add(twitchRedirectUrl);
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest), httpListener);
        }

        /// <summary>
        /// Handles the incoming HTTP request
        /// </summary>
        /// <param name="result"></param>
        private void IncomingHttpRequest(IAsyncResult result)
        {
            string code;
            string state;
            HttpListener httpListener;
            HttpListenerContext httpContext;
            HttpListenerRequest httpRequest;
            HttpListenerResponse httpResponse;
            string responseString;

            // get back the reference to our http listener
            httpListener = (HttpListener)result.AsyncState;

            // fetch the context object
            httpContext = httpListener.EndGetContext(result);

            // if we'd like the HTTP listener to accept more incoming requests, we'd just restart the "get context" here:
            // httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest),httpListener);
            // however, since we only want/expect the one, single auth redirect, we don't need/want this, now.
            // but this is what you would do if you'd want to implement more (simple) "webserver" functionality
            // in your project.

            // the context object has the request object for us, that holds details about the incoming request
            httpRequest = httpContext.Request;

            code = httpRequest.QueryString.Get("code");
            state = httpRequest.QueryString.Get("state");

            // check that we got a code value and the state value matches our remembered one
            if ((code.Length > 0) && (state == _twitchAuthStateVerify))
            {
                // if all checks out, use the code to exchange it for the actual auth token at the API
                GetTokenFromCode(code);
            }

            // build a response to send an "ok" back to the browser for the user to see
            httpResponse = httpContext.Response;

            // RESPONSE DISPLAYED ON WEBPAGE BY LOCAL SERVER
            responseString = "<html><body><b>DONE!</b><br>(You can close this tab/window now)</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            // send the output to the client browser
            httpResponse.ContentLength64 = buffer.Length;
            System.IO.Stream output = httpResponse.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

            // the HTTP listener has served it's purpose, shut it down
            httpListener.Stop();
            // obv. if we had restarted the waiting for more incoming request, above, we'd not Stop() it here.
        }

        /// <summary>
        /// Makes the API call to exchange the received code for the actual auth token
        /// </summary>
        /// <param name="code">The code parameter received in the callback HTTP reuqest</param>
        private void GetTokenFromCode(string code)
        {
            string apiUrl;
            string apiResponseJson;
            TwitchApiCodeTokenResponse apiResponseData;

            // construct full URL for API call
            apiUrl = "https://id.twitch.tv/oauth2/token" +
                     "?client_id=" + twitchClientId +
                     "&client_secret=" + twitchClientSecret +
                     "&code=" + code +
                     "&grant_type=authorization_code" +
                     "&redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl);

            // make sure our API helper knows our client ID (it needed for the HTTP headers)
            twitchApiCallHelper.TwitchClientId = twitchClientId;

            // make the call!
            apiResponseJson = twitchApiCallHelper.CallApi(apiUrl, "POST");

            // parse the return JSON into a more usable data object
            apiResponseData = JsonUtility.FromJson<TwitchApiCodeTokenResponse>(apiResponseJson);

            // fetch the tokens from the response data
            if (apiResponseData.access_token == null) // request failed
                return;

            _authToken = apiResponseData.access_token;
            _refreshToken = apiResponseData.refresh_token;
        }

        /// <summary>
        /// Updates the UI in the scene with the current auth data
        /// </summary>
        private void UpdateDisplay()
        {
            if (uiTextToken) uiTextToken.text = _authToken;
            if (uiTextuserId) uiTextuserId.text = _userId.ToString();
            if (uiTextusername) uiTextusername.text = _username;
            if (uiTextProfileName) uiTextProfileName.text = _username;
            if (uiTextViewcount) uiTextViewcount.text = broadcaster.ViewCount > 0 ? broadcaster.ViewCount.ToString() : "";
            if (uiTextProfilPicUrl) uiTextProfilPicUrl.text = broadcaster.ProfileImageUrl;
            if (uiTextemail) uiTextemail.text = broadcaster.Email;
            if (uiImageProfilePicture) uiImageProfilePicture.sprite = _profilePicture;

            bool ready = twitchClientId.Length > 0 && !twitchClientId.Equals("PUT YOUR CLIENT ID HERE") && twitchClientSecret.Length > 0 && !twitchClientSecret.Equals("PUT YOUR CLIENT SECRET HERE");

            mainButton.SetActive(!isIdentified && ready);
            if (runningPanel) runningPanel.SetActive(isRunning);

            if (mainTitle) mainTitle.SetActive(ready);
            if (mainTitlePrompt) mainTitlePrompt.SetActive(!ready);
            if (credentialSavedPrompt) credentialSavedPrompt.SetActive(ready && !isRunning && isIdentified);
        }

        /// <summary>
        /// This is a small necessary evil, as we can't update the UI directly from the HTTP handler.
        /// It's a Unity thing and has to do with the fact that internally the HTTP handler runs on a separate thread.
        /// Nothing you need to worry about. Just Unity stuff, lol.
        /// </summary>
        /// <returns></returns>
        IEnumerator DisplayUpdater()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                UpdateDisplay();
            }
        }

        /// <summary>
        /// If this method finishes right, user infos are validated --> credentials can be saved.
        /// </summary>
        /// <returns></returns>
        IEnumerator ValidateUserInfo()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://id.twitch.tv/oauth2/validate");
            www.SetRequestHeader("Authorization", "Bearer " + _authToken);
            yield return www.SendWebRequest();

#if UNITY_2020 || UNITY_2020_3_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                isIdentified = false;
                isRunning = false;
                yield break;
            }
#else
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                isIdentified = false;
                isRunning = false;
                yield break;
            }
#endif
            // Get results as text
            string jsonText = www.downloadHandler.text;

            // We don't need the UWR anymore
            www.Dispose();

            UserValidateResponse response = new UserValidateResponse();

            // parse the retrieved JSON object to our custom object
            response = JsonUtility.FromJson<UserValidateResponse>(jsonText);

            _userId = int.Parse(response.user_id);
            _username = response.login;

            isIdentified = true;

            Api api = new Api();

            api.Settings.AccessToken = _authToken;
            api.Settings.ClientId = twitchClientId;

            TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse getUsersResponse = null;

            yield return api.InvokeAsync(
                api.Helix.Users.GetUsersAsync(ids: new List<string> { _userId.ToString() }),
                (response2) => getUsersResponse = response2);
            // ===============================================
            // we will reach this point only when async request is done

            if (getUsersResponse.Users.Length > 0)
            {
                broadcaster = getUsersResponse.Users[0];
            }
            else
            {
                isIdentified = false;
                isRunning = false;
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
                LoadImage(texture);
            }

            Secrets.channel_name = broadcaster.Login; //broadcaster.DisplayName;
            Secrets.channel_id = broadcaster.Id;
            Secrets.game_client_id = twitchClientId;
            Secrets.game_client_secret = twitchClientSecret;
            Secrets.game_oauth_token = _authToken;
            Secrets.game_oauth_refresh_token = _refreshToken;
            Secrets.SetConnectionSuccess(true);

            Secrets.SaveSecrets();

            isRunning = false;
            isDoneSuccess = true;

            Debug.Log("[Twitch OAuth] Connection credentials succesfully retrieved and saved in PlayerPrefs.");

            if (TwitchManager.instance != null)
            {
                Debug.Log("[Twitch OAuth] A TwitchManager istance has been found, instructing it to connect now.");
                TwitchManager.instance.Connect();
            }
        }
    }
}

