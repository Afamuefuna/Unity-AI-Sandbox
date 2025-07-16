using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class GPTCommand : MonoBehaviour
{
    [Header("GPT Settings")]
    [SerializeField]
    private string model = "gpt-3.5-turbo"; // OpenAI model name to use

    [SerializeField]
    private string apiKey = "YOUR_API_KEY_HERE"; // API key for OpenAI

    void Start()
    {
        // Loads the actual API key stored in a Resources folder asset named "APIKey"
        TextAsset apiKeyAsset = Resources.Load<TextAsset>("APIKey");
        apiKey = apiKeyAsset.text;
    }

    // Public method to initiate the process of sending a prompt to ChatGPT
    public void SendPromptToChatGPT(string userPrompt)
    {
        StartCoroutine(SendChatGPTRequest(userPrompt));
    }

    // Sends the prompt to ChatGPT API and handles the response
    IEnumerator SendChatGPTRequest(string userPrompt)
    {
        // Constructs the full prompt with instruction formatting and expected JSON output
        string prompt = $"Parse this instruction: \"{userPrompt}\"\n" +
                        "Only include the boxes mentioned, if box is not mentioned, do not include it, if box is mentioned but color is not mentioned, do not include it. Return as JSON using hex color codes like:\n" +
                        "{ \"box 1\": \"#FF0000\", \"box 2\": \"#00FF00\", \"box 3\": \"#0000FF\" }";

        // Prepares the request payload for the OpenAI API
        var requestBody = new
        {
            model = model,
            messages = new[] {
                new { role = "system", content = "You are a Unity assistant. Return only JSON with box names and hex color codes like #FF0000. No explanation." },
                new { role = "user", content = prompt }
            }
        };

        // Serializes the request payload into a JSON string
        string jsonString = JsonConvert.SerializeObject(requestBody);

        // Sets up the UnityWebRequest to send the POST request to OpenAI
        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            // Waits for the API request to complete
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Logs and processes the successful response
                string response = request.downloadHandler.text;
                Debug.Log("Response: " + response);

                // Extracts only the JSON portion from the API response
                string extractedJson = ExtractJSON(response);

                // Applies the extracted colors to the corresponding GameObjects
                ApplyColors(extractedJson);
            }
            else
            {
                // Logs any error from the request
                Debug.LogError("OpenAI API Error: " + request.error);
            }
        }
    }

    // Parses a JSON string and applies the specified hex color to each named GameObject
    void ApplyColors(string jsonString)
    {
        try
        {
            var colorMap = JObject.Parse(jsonString);
            foreach (var pair in colorMap)
            {
                string boxName = pair.Key;
                string hexColor = pair.Value.ToString();

                // Finds the GameObject with the specified name
                GameObject box = GameObject.Find(boxName);

                if (box != null)
                {
                    // Tries to get the Renderer and apply the parsed color
                    Renderer renderer = box.GetComponent<Renderer>();
                    if (renderer != null && ColorUtility.TryParseHtmlString(hexColor, out Color color))
                    {
                        renderer.material.color = color;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid hex color: {hexColor} for {boxName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Box '{boxName}' not found.");
                }
            }
        }
        catch (System.Exception e)
        {
            // Logs if the JSON is invalid or parsing fails
            Debug.LogError("JSON parsing error: " + e.Message);
        }
    }

    // Extracts the actual JSON object (with box-color pairs) from the full ChatGPT response
    string ExtractJSON(string rawResponse)
    {
        try
        {
            JObject root = JObject.Parse(rawResponse);
            string content = root["choices"]?[0]?["message"]?["content"]?.ToString();

            if (content != null)
            {
                // Finds the start and end of the JSON object in the string
                int firstBrace = content.IndexOf('{');
                int lastBrace = content.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    // Returns the substring that represents the JSON object
                    return content.Substring(firstBrace, lastBrace - firstBrace + 1);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error extracting JSON: " + e.Message);
        }
        return "{}"; // Fallback in case of error
    }
}
