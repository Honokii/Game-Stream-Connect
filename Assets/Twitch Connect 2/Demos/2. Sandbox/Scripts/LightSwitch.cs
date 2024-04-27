using UnityEngine;

namespace TwitchConnect.Demo.Sandbox
{
    [RequireComponent(typeof(Light))]
    public class LightSwitch : MonoBehaviour
    {
        private Light mainLight;

        public void SwitchLight(int userID, string userMessage, object payload)
        {
            mainLight = mainLight == null ? GetComponent<Light>() : mainLight;
            mainLight.enabled = !mainLight.enabled;
        }
    }
}

