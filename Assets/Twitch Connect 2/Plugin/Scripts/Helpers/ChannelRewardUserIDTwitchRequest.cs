using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwitchConnect
{
    [System.Serializable]
    public class ChannelRewardUserIDTwitchRequest : MonoBehaviour
    {
        public object sender;
        public TwitchLib.PubSub.Events.OnRewardRedeemedArgs e;
        public Trigger_NewCustomReward_Any source;

        public void StartTwitchRequest(object _sender, TwitchLib.PubSub.Events.OnRewardRedeemedArgs _e, Trigger_NewCustomReward_Any _source)
        {
            sender = _sender;
            source = _source;
            e = _e;

            if (TwitchManager.instance != null)
                StartCoroutine(RequestUserIDThenRaiseChannelReward());
        }

        public IEnumerator RequestUserIDThenRaiseChannelReward()
        {
            TwitchLib.Api.Helix.Models.Users.GetUsers.GetUsersResponse getUsersResponse = null;

            yield return TwitchManager.instance.api.InvokeAsync(
                TwitchManager.instance.api.Helix.Users.GetUsersAsync(logins: new List<string> { e.Login }),
                                          (response) => getUsersResponse = response);

            UserDictionnary.AddToDictionnary(int.Parse(getUsersResponse.Users[0].Id), getUsersResponse.Users[0].Login);

            source.OnChannelRewardCallback(sender, int.Parse(getUsersResponse.Users[0].Id), e);

            Destroy(gameObject);
        }
    }
        
}