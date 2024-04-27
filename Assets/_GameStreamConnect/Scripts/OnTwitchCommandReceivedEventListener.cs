using System;
using UnityEngine.Events;
using Nemuge.GameEvent;

//Listener
public class OnTwitchCommandReceivedEventListener : BaseGameEventListener<OnTwitchCommandReceivedEventArgs, OnTwitchCommandReceivedEvent, OnTwitchCommandReceivedEventResponse> { }

//Response
[Serializable]
public class OnTwitchCommandReceivedEventResponse : UnityEvent<OnTwitchCommandReceivedEventArgs> { }