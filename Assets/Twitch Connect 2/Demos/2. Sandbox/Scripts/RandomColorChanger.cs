using UnityEngine;

namespace TwitchConnect.Demo.Sandbox
{
    public class RandomColorChanger : MonoBehaviour
    {

        public void ChangeColorRandom()
        {
            GetComponent<Renderer>().material.color = GenerateRandomColor();
        }

        private Color GenerateRandomColor()
        {
            Color randomColor = new Vector4(
                Random.Range(0.33f, 1f),    //red
                Random.Range(0.33f, 1f),    //green
                Random.Range(0.33f, 1f),    //blue
                1f);

            return randomColor;
        }
    }
}

