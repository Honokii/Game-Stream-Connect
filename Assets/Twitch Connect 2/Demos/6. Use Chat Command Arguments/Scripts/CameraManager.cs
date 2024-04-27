using UnityEngine;

namespace TwitchConnect.Demo.UseChatCommandArguments
{
    public class CameraManager : MonoBehaviour
    {
        public GameCamera[] cameras;

        private void Start()
        {
            if (cameras.Length == 0)
                return;

            foreach (GameCamera cam in cameras)
            {
                cam.InitializeCamera();
            }
        }

        /// <summary>
        /// Method that van be called by the Invoke Method action type from inspector. All arguments will be provided.
        /// </summary>
        /// <param name="userID">The ID of the chatter</param>
        /// <param name="userMessage">Its message as it was seen in chat</param>
        /// <param name="payload">The actual event data that you can cast to get more info</param>
        public void SwitchCameraFromChat(int userID, string userMessage, object payload)
        {
            // in case there are no camera
            if (cameras.Length == 0)
                return;

            // try to cast the Twitch trigger payload as the type produced by a Trigger_ChatCommand class 
            TwitchLib.Client.Events.OnChatCommandReceivedArgs e = payload as TwitchLib.Client.Events.OnChatCommandReceivedArgs;

            // check if the cast resulted in something. If not then do nothing
            if (e == null)
                return;

            // check if there is an argument. If not then do nothing
            if (e.Command.ArgumentsAsList.Count == 0)
                return;

            // extract the first argument of the command. Note that there can be as many as you like, all separated by whitespaces in chat
            string argument = e.Command.ArgumentsAsList[0];

            for (int i = 0; i < cameras.Length; i++)
            {
                // simply pass the argument to each GameCamera.
                // The GameCamera will compare the argument to its own to know if it should enable or not.
                // Not very s.o.l.i.d. but this is mostly for clarity here
                cameras[i].SetCameraActive(argument);
            }
        }
    }
}

