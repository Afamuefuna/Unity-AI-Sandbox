using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Static utility class to parse JSON and apply hex colors to named GameObjects.
/// </summary>
public static class Utility
{
    /// <summary>
    /// Parses a JSON string and applies the specified hex color to each named GameObject.
    /// Example input: { "Box1": "#FF0000", "Box2": "#00FF00" }
    /// </summary>
    /// <param name="jsonString">JSON string with GameObject name-color pairs</param>
    public static void ApplyColors(string jsonString)
    {
        try
        {
            var colorMap = JObject.Parse(jsonString);

            foreach (var pair in colorMap)
            {
                string boxName = pair.Key;
                string hexColor = pair.Value.ToString();

                GameObject box = GameObject.Find(boxName);

                if (box != null)
                {
                    Renderer renderer = box.GetComponent<Renderer>();

                    if (renderer != null && ColorUtility.TryParseHtmlString(hexColor, out Color color))
                    {
                        renderer.material.color = color;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid hex color '{hexColor}' for GameObject '{boxName}'.");
                    }
                }
                else
                {
                    Debug.LogWarning($"GameObject named '{boxName}' not found.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse JSON or apply colors: " + e.Message);
        }
    }

    /// <summary>
    /// Extracts the JSON object (with name-color pairs) from a GPT response.
    /// Looks for the first '{' and last '}' in the message content.
    /// </summary>
    /// <param name="rawResponse">Raw JSON response from GPT API</param>
    /// <returns>JSON substring containing GameObject-color mappings</returns>
    public static string ExtractJSON(string rawResponse)
    {
        try
        {
            JObject root = JObject.Parse(rawResponse);
            string content = root["choices"]?[0]?["message"]?["content"]?.ToString();

            if (content != null)
            {
                int firstBrace = content.IndexOf('{');
                int lastBrace = content.LastIndexOf('}');

                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    return content.Substring(firstBrace, lastBrace - firstBrace + 1);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to extract JSON: " + e.Message);
        }

        return "{}"; // fallback
    }
}
