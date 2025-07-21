using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.Events;
using System;

namespace DrawDash
{
    public class GPTCommand : MonoBehaviour
    {
        [Header("OpenAI Settings")]
        [SerializeField] private string apiKey = "YOUR_API_KEY";
        [SerializeField] private string model = "gpt-4";

        void Start()
        {
            TextAsset apiKeyAsset = Resources.Load<TextAsset>("APIKey");
            apiKey = apiKeyAsset.text;
        }

        public IEnumerator GetSketchPrompt(Action<string, int> onSuccess = null, Action<string> onError = null)
        {
    string userPrompt = "Give me one fun and simple thing to sketch quickly, like 'A cat', 'A house', 'A tree', 'A car' etc. Just reply with a JSON object containing the prompt and a countdown time in seconds, max time is 120, min time is 60, like this: { \\\"prompt\\\": \\\"A cat\\\", \\\"countdown\\\": 30 }";

            string jsonBody = $@"
{{
  ""model"": ""{model}"",
  ""messages"": [
    {{
      ""role"": ""user"",
      ""content"": ""{userPrompt}""
    }}
  ],
  ""max_tokens"": 20
}}";

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
                    string response = request.downloadHandler.text;
                    (string prompt, int countdown) = Utility.ExtractPromptAndCountdown(response);
                    onSuccess?.Invoke(prompt, countdown);
                }
                else
                {
                    Debug.LogError("OpenAI Error: " + request.error);
                    onError?.Invoke(request.error);
                }
            }
        }
    }
}
