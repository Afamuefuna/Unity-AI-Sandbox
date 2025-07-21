using UnityEngine;
using UnityEngine.UI;

namespace DrawDash
{
    public class SimpleDraw : MonoBehaviour
    {
        [Header("Drawing Settings")]
        public RawImage drawImage;
        public Color drawColor = Color.black;
        public int brushSize = 5;
        public Camera uiCamera;

        [Header("Performance Settings")]
        public float updateInterval = 0.016f;

        private Texture2D drawTexture;
        private Vector2 lastDrawPosition;
        private bool isDrawing = false;
        private float lastUpdateTime = 0f;
        private bool needsUpdate = false;

        void Start()
        {
            InitializeTexture();
        }

        void InitializeTexture()
        {
            drawTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            drawTexture.filterMode = FilterMode.Point;
            drawImage.texture = drawTexture;
            ClearTexture();
        }

        void Update()
        {
            HandleInput();
            UpdateTexture();
        }

        void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartDrawing();
            }
            else if (Input.GetMouseButton(0) && isDrawing)
            {
                ContinueDrawing();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                StopDrawing();
            }
        }

        void StartDrawing()
        {
            Vector2 textureCoord = GetTextureCoordinate();
            if (IsValidCoordinate(textureCoord))
            {
                isDrawing = true;
                lastDrawPosition = textureCoord;
                DrawAtPosition(textureCoord);
            }
        }

        void ContinueDrawing()
        {
            Vector2 textureCoord = GetTextureCoordinate();
            if (IsValidCoordinate(textureCoord))
            {
                // Draw a line from last position to current position for smooth strokes
                DrawLine(lastDrawPosition, textureCoord);
                lastDrawPosition = textureCoord;
            }
        }

        void StopDrawing()
        {
            isDrawing = false;
            // Force immediate update when stopping
            if (needsUpdate)
            {
                drawTexture.Apply();
                needsUpdate = false;
            }
        }

        Vector2 GetTextureCoordinate()
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawImage.rectTransform,
                Input.mousePosition,
                uiCamera,
                out localPos
            );

            // Convert to texture coordinates
            Vector2 textureCoord = new Vector2(
                (localPos.x + drawImage.rectTransform.rect.width / 2) * drawTexture.width / drawImage.rectTransform.rect.width,
                (localPos.y + drawImage.rectTransform.rect.height / 2) * drawTexture.height / drawImage.rectTransform.rect.height
            );

            return textureCoord;
        }

        bool IsValidCoordinate(Vector2 coord)
        {
            return coord.x >= 0 && coord.x < drawTexture.width &&
                   coord.y >= 0 && coord.y < drawTexture.height;
        }

        void DrawLine(Vector2 start, Vector2 end)
        {
            float distance = Vector2.Distance(start, end);
            int steps = Mathf.CeilToInt(distance);

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 point = Vector2.Lerp(start, end, t);
                DrawAtPosition(point);
            }
        }

        void DrawAtPosition(Vector2 texPos)
        {
            // Circular brush for more natural drawing
            for (int x = -brushSize; x <= brushSize; x++)
            {
                for (int y = -brushSize; y <= brushSize; y++)
                {
                    // Check if point is within circular brush
                    if (x * x + y * y <= brushSize * brushSize)
                    {
                        int px = (int)texPos.x + x;
                        int py = (int)texPos.y + y;

                        if (px >= 0 && px < drawTexture.width && py >= 0 && py < drawTexture.height)
                        {
                            drawTexture.SetPixel(px, py, drawColor);
                            needsUpdate = true;
                        }
                    }
                }
            }
        }

        void UpdateTexture()
        {
            if (needsUpdate && Time.time - lastUpdateTime >= updateInterval)
            {
                drawTexture.Apply();
                needsUpdate = false;
                lastUpdateTime = Time.time;
            }
        }

        public void ClearTexture()
        {
            Color[] clearPixels = new Color[drawTexture.width * drawTexture.height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.white;
            }
            drawTexture.SetPixels(clearPixels);
            drawTexture.Apply();
        }

        // Public methods for UI integration
        public void SetDrawColor(Color color)
        {
            drawColor = color;
        }

        public void SetBrushSize(int size)
        {
            brushSize = Mathf.Clamp(size, 1, 50);
        }

        public void SetBrushSize(float size)
        {
            SetBrushSize((int)size);
        }

        // Save texture to file (optional)
        public byte[] SaveTexture()
        {
            byte[] bytes = drawTexture.EncodeToPNG();
            string filename = "drawing_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            System.IO.File.WriteAllBytes("Assets/Resources/" + filename, bytes);
            return bytes;
        }

        void OnDestroy()
        {
            if (drawTexture != null)
            {
                DestroyImmediate(drawTexture);
            }
        }
    }
}