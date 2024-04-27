using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace TwitchConnect.Demo.TwitchRoss
{
    public class CanvasTexture : MonoBehaviour
    {
        public GameManager game;

        // Texture representation of the canvas
        public Texture2D texture;
        // Live display of the texture
        RenderTexture renderTexture;

        public Material material;
        public float borderSize = 45;

        public float autoSaveTimeMinutes = 10;
        private float autoSaveTimeSeconds;
        private float remainingTimeBeforeSaveSeconds;

        private string savePath;

        RectTransform m_Rtransform;

        // Start is called before the first frame update
        void Start()
        {
            renderTexture = (RenderTexture)material.mainTexture;
            CreateTexture(true);
            AdjustSize();

            autoSaveTimeSeconds = autoSaveTimeMinutes * 60;
            remainingTimeBeforeSaveSeconds = autoSaveTimeSeconds;

            m_Rtransform = GetComponent<RawImage>().rectTransform;

            savePath = Application.streamingAssetsPath;
        }

        private void Update()
        {
            remainingTimeBeforeSaveSeconds -= Time.deltaTime;
            if (remainingTimeBeforeSaveSeconds < 0)
            {
                remainingTimeBeforeSaveSeconds = autoSaveTimeSeconds;
                SaveImage();
            }

            Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (Input.GetMouseButton(0) && RectTransformUtility.RectangleContainsScreenPoint(m_Rtransform, mouseScreenPosition))
            {
                Vector3[] corners = new Vector3[4];
                m_Rtransform.GetWorldCorners(corners);
                Vector2 mouseCanvasPosition;
                mouseCanvasPosition = Input.mousePosition - corners[0];
                mouseCanvasPosition.x = mouseCanvasPosition.x * renderTexture.width / m_Rtransform.rect.width;
                mouseCanvasPosition.y = mouseCanvasPosition.y * renderTexture.height / m_Rtransform.rect.height;
                Color col = Color.green;
                // place only for first click or keep on placing with space bar + click
                 if (Input.GetKey(KeyCode.Space) || Input.GetMouseButtonDown(0))
                    game.MousePlacePixel(mouseCanvasPosition, col, false);
            }
        }

        public void PlaceRandomPixel()
        {
            PlacePixel(Random.ColorHSV(), new Vector2(Random.Range(0, renderTexture.width), Random.Range(0, renderTexture.height)));
        }

        public void ClearPixels()
        {
            CreateTexture(true);
        }

        public void SaveImage()
        {
            SaveTexture(texture, savePath);
            game.AddToConsole("*Save*");
        }

        public void LoadImage()
        {
            LoadTexture(savePath);
        }

        public bool PlacePixel(Color c, Vector2 pos)
        {
            if (pos.x > renderTexture.width || pos.y > renderTexture.height)
                return false;

            texture.SetPixel((int)pos.x, (int)pos.y, c);
            texture.Apply();
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture, renderTexture);
            RenderTexture.active = oldRT;

            return true;
        }

        void CreateTexture(bool reset)
        {
            texture = new Texture2D(renderTexture.width, renderTexture.height);

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            if (reset)
            {
                Color[] pixels = new Color[texture.width * texture.height];

                for (int x = 0; x < texture.width; x++)
                {
                    for (int y = 0; y < texture.height; y++)
                    {
                        pixels[x + y * texture.width] = Color.white;
                    }
                }
                texture.SetPixels(pixels);
            }
            else
            {
                texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            }

            texture.Apply();

            RenderTextureToCanvas();
        }

        void RenderTextureToCanvas()
        {
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Graphics.Blit(texture, renderTexture);
            RenderTexture.active = oldRT;
        }

        void LoadTexture(string dirPath)
        {
            DirectoryInfo info = new DirectoryInfo(dirPath);
            FileInfo[] fileInfo = info.GetFiles("*.png");
            if (fileInfo.Length == 0)
                return;

            FileInfo latest = fileInfo[0];

            foreach (FileInfo f in fileInfo)
            {
                if (latest.CreationTimeUtc < f.CreationTimeUtc)
                    latest = f;
            }

            // read image and store in a byte array
            byte[] byteArray = File.ReadAllBytes(latest.ToString());

            //create a texture and load byte array to it
            Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height);
            // the size of the texture will be replaced by image size
            bool isLoaded = tempTexture.LoadImage(byteArray, true);
            // load image if succesfully read
            if (isLoaded)
            {
                texture = duplicateTexture(tempTexture);
                RenderTextureToCanvas();
            }
        }

        Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        void SaveTexture(Texture2D texture, string dirPath)
        {
            byte[] bytes = texture.EncodeToPNG();
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "/R_" + Random.Range(0, 100000) + ".png", bytes);
            Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        void AdjustSize()
        {
            float screenHeight = Screen.height;
            float canvasSize = screenHeight - (borderSize * 2);
            float canvasXposition = canvasSize / 2 + borderSize;

            RectTransform rt = GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasSize);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasSize);

            rt.anchoredPosition = new Vector3(canvasXposition, rt.anchoredPosition.y);
        }
    }
}
