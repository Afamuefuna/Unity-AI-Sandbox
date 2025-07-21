using System;
using System.Collections;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace DrawDash
{
  public class GPTVision : MonoBehaviour
  {
    [TextArea(3, 10)]
    public string openAIResponse;

    [Header("Set Your API Key")]
    public string apiKey = "YOUR_API_KEY_HERE";

    void Start()
    {
      TextAsset apiKeyAsset = Resources.Load<TextAsset>("APIKey");
      apiKey = apiKeyAsset.text;
    }

    public IEnumerator SendImageToOpenAI(byte[] pngBytes, string prompt, Action<string> onSuccess = null, Action<string> onError = null)
    {
      string base64Image = System.Convert.ToBase64String(pngBytes);

      // Prompt that simplifies the judgment criteria
      string userPrompt = $"A user attempted to draw '{prompt}'. Examine the sketch closely." +
                          $" If the image is blank return 0, scribbled, or does not show a recognizable representation of the prompt, rate it very low (1 or 2). " +
                          $"If it clearly shows the object with correct shape or features, rate it higher. Return ONLY a single number score from 1 to 10. Do not explain." +
                          $"it is just a white and black sketch, not a complete drawing so do not be harsh on your ratings. ";

      // Compose the JSON body
      string jsonBody = $@"
{{
  ""model"": ""gpt-4o"",
  ""messages"": [
    {{
      ""role"": ""user"",
      ""content"": [
        {{
          ""type"": ""text"",
          ""text"": ""{userPrompt}""
        }},
        {{
          ""type"": ""image_url"",
          ""image_url"": {{
            ""url"": ""data:image/png;base64,{base64Image}""
          }}
        }}
      ]
    }}
  ],
  ""max_tokens"": 10
}}";

      // Send request
      using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
      {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
          openAIResponse = request.downloadHandler.text;
          string score = Utility.ExtractContentJSON(openAIResponse);

          if (onSuccess != null)
          {
            onSuccess(score);
          }
        }
        else
        {
          Debug.LogError("Error: " + request.error);

          if (onError != null)
          {
            onError(request.error);
          }
        }
      }
    }
  }

}
