using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Contains all settings for the TwitchManager
    /// </summary>
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Make this script a singleton. Change script to be a static class if needed.")]
        /// <summary>
        /// Make this script a singleton.
        /// </summary>
        public bool singleton = false;

        [Tooltip("Show full message sent and received from Twitch API.")]
        /// <summary>
        /// Show full message sent and received from Twitch API.
        /// </summary>
        public bool verbose = false;

        [Tooltip("Character that all commands begin with. Exemple : '!'.")]
        /// <summary>
        /// Character that all commands begin with. Exemple : '!'.
        /// </summary>
        public char commandPrefix = '!';

        [Tooltip("Return list of commands if 'commands' command is sent in chat (prefixed by commandPrefix).")]
        /// <summary>
        /// Return list of commands if 'commands' command is sent in chat (prefixed by commandPrefix).
        /// </summary>
        public bool listCommand = true;

        [Tooltip("Return custom help message in chat if 'help' command is sent in chat (prefixed by commandPrefix).")]
        /// <summary>
        /// Return list of commands if 'help' command is sent in chat (with command prefix).
        /// </summary>
        public bool helpCommand = true;

        [TextArea]
        [Tooltip("Custom Help Message sent in chat if help command is sent.")]
        /// <summary>
        /// Custom Help Message sent in chat if help command is sent..
        /// </summary>
        public string helpMessage = "";

        [Tooltip("Use the built-in onGUI admin panel.")]
        /// <summary>
        /// Use the onGUI admin panel.
        /// </summary>
        public bool builtinGui = true;

        [Tooltip("Keypress to use to display Twitch settings GUI in-game.")]
        /// <summary>
        /// Keypress used to display Twitch settings GUI in-game;
        /// </summary>
        public KeyCode uIKeyCode = KeyCode.F12;

        [HideInInspector]
        /// <summary>
        /// Shall the GUI be shown atm ?
        /// </summary>
        public bool showGUI;

        [Tooltip("Connect automatically on start of this monobehaviour.")]
        /// <summary>
        /// Connect automatically on start of this monobehaviour.
        /// </summary>
        public bool autoConnect;

        [Tooltip("Request Unity to repaint this inspector each editor frame (fixed 30fps) ? Warning: significant performance drawback in playmode.")]
        /// <summary>
        /// Request Unity to repaint this inspector each editor frame (fixed 30fps) ? Warning: significant performance drawback in playmode.
        /// </summary>
        public bool editorConstantRepaint;

        // internal flag to broadcast twitch chat
        internal bool broadcastTwitchChat;

        // internal flag for bot connection
        public bool useBot;
    }
}


