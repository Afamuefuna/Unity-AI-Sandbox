using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RuntimeSpritesheetAnimator : MonoBehaviour
{
    [Header("Spritesheet Configuration")]
    public string spritesheetURL = "https://example.com/character_walk.png";
    public int frameWidth = 256;
    public int frameHeight = 1024;
    public int framesPerRow = 4;
    public int totalFrames = 4;
    
    [Header("Animation Settings")]
    public float frameRate = 12f;
    public bool loop = true;
    public bool playOnStart = true;
    
    [Header("Save Settings")]
    public bool saveFramesToResources = false;
    public string saveFolder = "AnimationFrames";
    public string filePrefix = "Frame_";
    
    private SpriteRenderer spriteRenderer;
    private Sprite[] animationFrames;
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;
    private bool isLoaded = false;
    
    // Events
    public System.Action OnAnimationComplete;
    public System.Action OnSpriteLoaded;
    public System.Action OnFramesSaved;

    [SerializeField] Texture2D sampleTexture;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        LoadSpritesheetFromTexture(sampleTexture);
    }
    
    void Update()
    {
        if (isPlaying && isLoaded)
        {
            UpdateAnimation();
        }
    }
    
    IEnumerator LoadSpritesheetFromURL()
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(spritesheetURL))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                CreateSpritesFromTexture(texture);
                isLoaded = true;
                OnSpriteLoaded?.Invoke();
                
                if (playOnStart)
                {
                    Play();
                }
            }
            else
            {
                Debug.LogError($"Failed to load spritesheet: {request.error}");
            }
        }
    }
    
    void CreateSpritesFromTexture(Texture2D texture)
    {
        // Ensure texture is readable
        if (!texture.isReadable)
        {
            Debug.LogError("Texture must be readable to create sprites at runtime");
            return;
        }
        
        animationFrames = new Sprite[totalFrames];
        
        for (int i = 0; i < totalFrames; i++)
        {
            // Calculate position in spritesheet
            int row = i / framesPerRow;
            int col = i % framesPerRow;
            
            // Create rect for this frame
            Rect frameRect = new Rect(
                col * frameWidth,
                texture.height - (row + 1) * frameHeight, // Flip Y coordinate
                frameWidth,
                frameHeight
            );
            
            // Create sprite
            animationFrames[i] = Sprite.Create(
                texture,
                frameRect,
                new Vector2(0.5f, 0.5f), // Pivot point (center)
                100f // Pixels per unit
            );
            
            animationFrames[i].name = $"Frame_{i}";
        }
        
        // Set the first frame
        if (animationFrames.Length > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }
        
        // Save frames to Resources folder if enabled
        if (saveFramesToResources)
        {
            SaveFramesToResources(texture);
        }
    }
    
    void SaveFramesToResources(Texture2D sourceTexture)
    {
#if UNITY_EDITOR
        // Create Resources folder path
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
            AssetDatabase.Refresh();
        }
        
        // Create subfolder for animation frames
        string savePath = Path.Combine(resourcesPath, saveFolder);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // Extract and save each frame
        for (int i = 0; i < totalFrames; i++)
        {
            // Calculate position in spritesheet
            int row = i / framesPerRow;
            int col = i % framesPerRow;
            
            // Calculate frame position (same as in CreateSpritesFromTexture)
            int x = col * frameWidth;
            int y = sourceTexture.height - (row + 1) * frameHeight; // Flip Y coordinate
            
            // Create new texture for this frame
            Texture2D frameTexture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGBA32, false);
            
            // Copy pixels from source texture
            Color[] pixels = sourceTexture.GetPixels(x, y, frameWidth, frameHeight);
            frameTexture.SetPixels(pixels);
            frameTexture.Apply();
            
            // Convert to PNG
            byte[] pngData = frameTexture.EncodeToPNG();
            
            // Save file
            string fileName = $"{filePrefix}{i:D2}.png";
            string filePath = Path.Combine(savePath, fileName);
            File.WriteAllBytes(filePath, pngData);
            
            // Cleanup temp texture
            DestroyImmediate(frameTexture);
            
            Debug.Log($"Saved frame {i} to: {filePath}");
        }
        
        // Refresh asset database to show new files
        AssetDatabase.Refresh();
        
        Debug.Log($"All frames saved to: {savePath}");
        OnFramesSaved?.Invoke();
        
        // Optionally, import the saved textures with correct settings
        ImportSavedFrames();
#else
        Debug.LogWarning("Frame saving only works in Unity Editor");
#endif
    }
    
#if UNITY_EDITOR
    void ImportSavedFrames()
    {
        string relativePath = Path.Combine("Assets/Resources", saveFolder);
        
        // Get all PNG files in the save directory
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { relativePath });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Get the texture importer
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            
            if (importer != null)
            {
                // Configure import settings for sprites
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f; // Match the pixels per unit from runtime creation
                importer.filterMode = FilterMode.Point; // For pixel art
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                
                // Apply and reimport
                importer.SaveAndReimport();
            }
        }
        
        Debug.Log("Frame import settings applied");
    }
#endif
    
    void UpdateAnimation()
    {
        timer += Time.deltaTime;
        
        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame++;
            
            if (currentFrame >= animationFrames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = animationFrames.Length - 1;
                    isPlaying = false;
                    OnAnimationComplete?.Invoke();
                    return;
                }
            }
            
            spriteRenderer.sprite = animationFrames[currentFrame];
        }
    }
    
    // Public methods to control animation
    public void Play()
    {
        if (isLoaded)
        {
            isPlaying = true;
        }
    }
    
    public void Pause()
    {
        isPlaying = false;
    }
    
    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        timer = 0f;
        
        if (animationFrames != null && animationFrames.Length > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }
    }
    
    public void SetFrame(int frameIndex)
    {
        if (animationFrames != null && frameIndex >= 0 && frameIndex < animationFrames.Length)
        {
            currentFrame = frameIndex;
            spriteRenderer.sprite = animationFrames[frameIndex];
        }
    }
    
    public void SetFrameRate(float newFrameRate)
    {
        frameRate = Mathf.Max(0.1f, newFrameRate);
    }
    
    // Public method to manually save frames
    [ContextMenu("Save Frames to Resources")]
    public void SaveFramesManually()
    {
        if (animationFrames != null && animationFrames.Length > 0)
        {
            SaveFramesToResources(animationFrames[0].texture);
        }
        else
        {
            Debug.LogWarning("No frames loaded. Load a spritesheet first.");
        }
    }
    
    // Alternative method for loading from byte array (useful for downloaded data)
    public void LoadSpritesheetFromBytes(byte[] imageData, int width, int height, int frames, int framesRow)
    {
        Texture2D texture = new Texture2D(width, height);
        if (texture.LoadImage(imageData))
        {
            frameWidth = width / framesRow;
            frameHeight = height / Mathf.CeilToInt((float)frames / framesRow);
            framesPerRow = framesRow;
            totalFrames = frames;
            
            CreateSpritesFromTexture(texture);
            isLoaded = true;
            OnSpriteLoaded?.Invoke();
            
            if (playOnStart)
            {
                Play();
            }
        }
        else
        {
            Debug.LogError("Failed to load texture from byte array");
        }
    }
    
    // Method for loading from existing texture
    public void LoadSpritesheetFromTexture(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogWarning("No texture provided");
            return;
        }
        
        CreateSpritesFromTexture(texture);
        isLoaded = true;
        OnSpriteLoaded?.Invoke();
        
        if (playOnStart)
        {
            Play();
        }
    }
    
    // Cleanup
    void OnDestroy()
    {
        if (animationFrames != null)
        {
            foreach (var sprite in animationFrames)
            {
                if (sprite != null)
                {
                    DestroyImmediate(sprite.texture);
                    DestroyImmediate(sprite);
                }
            }
        }
    }
}