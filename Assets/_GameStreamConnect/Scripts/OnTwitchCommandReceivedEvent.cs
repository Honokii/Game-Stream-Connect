using UnityEngine;
using Nemuge.GameEvent;
using TwitchLib.Client.Models;

//Event
//Set the menu name as you see fit.
[CreateAssetMenu(fileName = "NewOnTwitchCommandReceivedEvent", menuName = "Nemuge/Game Events/OnTwitchCommandReceivedEvent")]
public class OnTwitchCommandReceivedEvent : BaseGameEvent<OnTwitchCommandReceivedEventArgs> { }

//Arguments
//Put parameters here if you want to pass that through the event
public class OnTwitchCommandReceivedEventArgs {
    public int UserID;
    public string UserMessage;
    public ChatCommand Payload;
}