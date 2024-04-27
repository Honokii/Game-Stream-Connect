using UnityEngine;

namespace TwitchConnect.Demo.TwitchRoss
{
    [System.Serializable]
    public class Pixel
    {
        public int userID;
        public string username;
        public Vector2 pos;
        public Color color;

        public Pixel(int _userID, string _username, Vector2 _pos, Color _color)
        {
            userID = _userID;
            username = _username;
            pos = _pos;
            color = _color;
        }
    }
}
