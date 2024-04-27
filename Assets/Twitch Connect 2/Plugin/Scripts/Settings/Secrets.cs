using UnityEngine;
/// <summary>
/// Contains all secrets to connect to twitch.
/// </summary>
public static class Secrets
{
    // Game credentials (broadcaster)
    public static string channel_name = ""; //          Broadcaster's channel name
    public static string channel_id = ""; //            Broadcaster's channel ID
    public static string game_client_id = ""; //        The App (id) under which the game authenticates against at Twitch
    public static string game_client_secret = ""; //        The App (id) under which the game authenticates against at Twitch
    public static string game_oauth_token = "";  //     The token used for API calls (linked to the client ID above)
    public static string game_oauth_refresh_token = "";  //     The token used for API calls (linked to the client ID above)


    // Bot credentials (to engage in chat as bot)
    public static string IRC_bot_name = ""; //          The name of the bot
    public static string IRC_bot_oauth = ""; //         The bot oauth token to post in chat. Get it at https://twitchapps.com/tmi/

    public static int last_Connection_Success = 0;

    public static void SaveSecrets()
    {
        if (channel_name.Length > 0)
            PlayerPrefs.SetString("twitch_channel_name", channel_name);
        if (channel_id.Length > 0)
            PlayerPrefs.SetString("twitch_channel_id", channel_id);
        if (game_client_id.Length > 0)
            PlayerPrefs.SetString("twitch_game_client_id", game_client_id);
        if (game_client_secret.Length > 0)
            PlayerPrefs.SetString("twitch_game_client_secret", game_client_secret);
        if (game_oauth_token.Length > 0)
            PlayerPrefs.SetString("twitch_game_oauth_token", game_oauth_token);
        if (game_oauth_refresh_token.Length > 0)
            PlayerPrefs.SetString("twitch_game_oauth_refresh_token", game_oauth_refresh_token);
        if (IRC_bot_name.Length > 0)
            PlayerPrefs.SetString("twitch_bot_name", IRC_bot_name);
        if (IRC_bot_oauth.Length > 0)
            PlayerPrefs.SetString("twitch_client_id", IRC_bot_oauth);          
    }

    public static void SetConnectionSuccess(bool success)
    {
        PlayerPrefs.SetInt("twitch_last_Connection_Success", success ? 1 : 0);
    }

    public static void LoadSecrets()
    {
        channel_name = PlayerPrefs.GetString("twitch_channel_name", channel_name);
        channel_id = PlayerPrefs.GetString("twitch_channel_id", channel_id);
        game_client_id = PlayerPrefs.GetString("twitch_game_client_id", game_client_id);
        game_client_secret = PlayerPrefs.GetString("twitch_game_client_secret", game_client_secret);
        game_oauth_token = PlayerPrefs.GetString("twitch_game_oauth_token", game_oauth_token);
        game_oauth_refresh_token = PlayerPrefs.GetString("twitch_game_oauth_refresh_token", game_oauth_refresh_token);
        IRC_bot_name = PlayerPrefs.GetString("twitch_IRC_bot_name", IRC_bot_name);
        IRC_bot_oauth = PlayerPrefs.GetString("twitch_IRC_bot_oauth", IRC_bot_oauth);
        last_Connection_Success = PlayerPrefs.GetInt("twitch_last_Connection_Success", last_Connection_Success);
    }

    public static void ClearSecrets()
    {
        PlayerPrefs.DeleteKey("twitch_channel_name");
        PlayerPrefs.DeleteKey("twitch_channel_id");
        PlayerPrefs.DeleteKey("twitch_game_client_id");
        PlayerPrefs.DeleteKey("twitch_game_client_secret");
        PlayerPrefs.DeleteKey("twitch_game_oauth_token");
        PlayerPrefs.DeleteKey("twitch_game_oauth_refresh_token");
        PlayerPrefs.DeleteKey("twitch_IRC_bot_name");
        PlayerPrefs.DeleteKey("twitch_IRC_bot_oauth");
        PlayerPrefs.DeleteKey("twitch_last_Connection_Success");
    }
}