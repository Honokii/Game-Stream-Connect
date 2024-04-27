using UnityEngine;


namespace TwitchConnect.Demo.Sandbox
{
    public class CubeSpawner : MonoBehaviour
    {
        public GameObject prefab;

        [Range(0.1f, 2f)]
        public float maxScale = 1f;

        public void SpawnCube()
        {
            GameObject go = Instantiate(prefab, transform.position, transform.rotation); // instantiate
            go.transform.localScale = new Vector3(Random.Range(0.05f, maxScale), Random.Range(0.05f, maxScale), Random.Range(0.05f, maxScale)); // scale
            go.GetComponent<Renderer>().material = GenerateNewRandomColorMaterial(); // random color
        }

        private Material GenerateNewRandomColorMaterial()
        {
            Color randomColor = new Vector4(
                Random.Range(0.33f, 1f),    //red
                Random.Range(0.33f, 1f),    //green
                Random.Range(0.33f, 1f),    //blue
                1f);

            Material result = new Material(Shader.Find("Standard"));
            result.color = randomColor;

            return result;
        }
    }
}

