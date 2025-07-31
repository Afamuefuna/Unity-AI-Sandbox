using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public class DallERequest
{
    public string model = "dall-e-3";
    public string prompt;
    public int n = 1;
    public string size = "1024x1024";
    public string quality = "hd";
    public string style = "vivid";
    public string response_format = "b64_json";
}

[System.Serializable]
public class DallEResponse
{
    public DallEImageData[] data;
}

[System.Serializable]
public class DallEImageData
{
    public string b64_json;
    public string revised_prompt;
}

public class ImageGen : MonoBehaviour
{
    [SerializeField] private string apiKey = "YOUR_OPENAI_API_KEY";
    private const string API_URL = "https://api.openai.com/v1/images/generations";
    
    void Start()
    {
        TextAsset apiKeyAsset = Resources.Load<TextAsset>("APIKey");
        apiKey = apiKeyAsset.text;

        StartCoroutine(GenerateImage());
    }
    
    IEnumerator GenerateImage()
    {
        DallERequest requestData = new DallERequest
        {
            prompt = "Create a 2D pixel art 4-frame sprite sheet of Kratos from the God of War series in a walking animation cycle. Each frame should illustrate a distinct stage of the walk: left foot forward, mid-stride, right foot forward, and returning stride. Kratos should be wearing his iconic Norse-era armor, with the Leviathan Axe visible on his back. Maintain a consistent 16-bit retro pixel art style. All frames should be aligned horizontally in a single row with equal spacing. The background must be fully transparent to allow seamless integration into game engines.",
            model = "dall-e-3",
            size = "1024x1024",
            response_format = "b64_json"
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    DallEResponse response = JsonUtility.FromJson<DallEResponse>(request.downloadHandler.text);
                    
                    if (response.data != null && response.data.Length > 0)
                    {
                        string imageBase64 = response.data[0].b64_json;
                        SaveImageFromBase64(imageBase64, "sprite.png");
                        
                        Debug.Log("Revised prompt: " + response.data[0].revised_prompt);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
                Debug.LogError("Response Code: " + request.responseCode);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }
    
    void SaveImageFromBase64(string base64String, string fileName)
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            //save to resources
            string filePath = Path.Combine(Application.dataPath, "Resources", fileName);
            File.WriteAllBytes(filePath, imageBytes);
            
            Debug.Log("Image saved to: " + filePath);
            
            StartCoroutine(LoadAndDisplayTexture(filePath));
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving image: " + e.Message);
        }
    }
    
    IEnumerator LoadAndDisplayTexture(string filePath)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + filePath))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Debug.Log($"Texture loaded: {texture.width}x{texture.height}");
                
                // Example: Apply to a SpriteRenderer if one exists
                var spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = Sprite.Create(texture, 
                        new Rect(0, 0, texture.width, texture.height), 
                        Vector2.one * 0.5f);
                }
            }
        }
    }
}