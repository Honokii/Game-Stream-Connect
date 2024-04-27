using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TwitchConnect;

namespace TwitchConnect.Demo.TwitchRoss
{
    public class GameManager : MonoBehaviour
    {
        public CanvasTexture textureManager;
        public TwitchOAuth twitchOAuth;

        public Text textLeaderboard;
        public Text textConsole;

        public List<Pixel> pixels;
        public Dictionary<string, int> pixelCountPerUser;
        public List<KeyValuePair<string, int>> leaderboard;

        // Start is called before the first frame update
        void Start()
        {
            pixels = new List<Pixel>();
            pixelCountPerUser = new Dictionary<string, int>();
            leaderboard = new List<KeyValuePair<string, int>>();

            textLeaderboard.text = "";
            textConsole.text = "";
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Return))
            {
                AddRandom();
            }
        }

        public void AddRandom()
        {
            PlacePixel(1, "OilyMcBrushface-" + Random.Range(1, 11),
                    new Vector2(Random.Range(1, 330),
                                Random.Range(1, 330)),
                    Random.ColorHSV());
        }

        public void ResetStats()
        {
            Start();
        }

        public void TryPlacePixel(int userID, string userMessage, object payload)
        {
            if (payload is TwitchLib.Client.Events.OnChatCommandReceivedArgs)
            {
                Color col = new Color();
                Vector2 pos = new Vector2();

                TwitchLib.Client.Events.OnChatCommandReceivedArgs e = payload as TwitchLib.Client.Events.OnChatCommandReceivedArgs;

                if (isArgsValid(e.Command.ArgumentsAsList, ref col, ref pos))
                {
                    PlacePixel(userID, e.Command.ChatMessage.Username, pos, col);
                }
            }

            
        }

        public void AddToConsole(string message)
        {
            string previousConsole = textConsole.text.ToString();
            textConsole.text = message;
            textConsole.text += '\n';
            textConsole.text += previousConsole;
            // trim excess of characters in console
            if (textConsole.text.Length > 997)
                textConsole.text = textConsole.text.Substring(0, 991);

        }

        public void MousePlacePixel(Vector2 pos, Color col, bool record = true)
        {
            if (twitchOAuth != null && twitchOAuth.isDoneSuccess)
            {
                PlacePixel(twitchOAuth._userId, twitchOAuth._username, pos, col, record);
            }
            else
            {
                PlacePixel(1, "Host", pos, col, record);
            }
        }

        void PlacePixel(int userID, string username, Vector2 pos, Color col, bool record = true)
        {
            if (textureManager.PlacePixel(col, pos))
            {
                pixels.Add(new Pixel(userID, username, pos, col));

                if (record)
                {
                    if (pixelCountPerUser.ContainsKey(username))
                        pixelCountPerUser[username]++;
                    else
                        pixelCountPerUser.Add(username, 1);

                    AddToConsole(username + " placed a pixel at (" + (int)(pos.x + 1) + ',' + (int)(pos.y + 1) + ')');
                }

                UpdateLeaderboard();
            }
        }

        bool isArgsValid(List<string> args, ref Color col, ref Vector2 pos)
        {
            if (args == null)
                return false;
            int numberOfArgs = 5;
            if (args.Count < numberOfArgs)
                return false;
            int[] intArgs = new int[numberOfArgs];
            for (int i = 0; i < numberOfArgs; i++)
            {
                intArgs[i] = int.Parse(args[i]);
                if (intArgs[i] == -1)
                {
                    return false;
                }
            }

            pos.x = Mathf.Clamp(intArgs[0], 1, textureManager.material.mainTexture.width) - 1;
            pos.y = Mathf.Clamp(intArgs[1], 1, textureManager.material.mainTexture.height) - 1;
            col.r = Mathf.Clamp01(intArgs[2] / 255);
            col.g = Mathf.Clamp01(intArgs[3] / 255);
            col.b = Mathf.Clamp01(intArgs[4] / 255);
            col.a = 1f;

            return true;
        }

        void UpdateLeaderboard()
        {
            leaderboard = pixelCountPerUser.ToList();
            leaderboard.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            string content = "";
            for (int i = 0; i < Mathf.Min(10, leaderboard.Count); i++)
            {
                content += (i + 1).ToString();
                content += ". ";
                content += leaderboard[i].Key;
                content += " with ";
                content += leaderboard[i].Value;
                content += " pixels placed";
                content += '\n';
            }
            textLeaderboard.text = content;
        }
    }
}
