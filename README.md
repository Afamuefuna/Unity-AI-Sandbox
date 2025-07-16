# Unity-AI-Sandbox

**Unity-AI-Sandbox** is a collection of experimental projects that explore the integration of Artificial Intelligence (AI) with the Unity game engine. It includes prototypes demonstrating how AI models like OpenAI's GPT can be used to drive interactivity, generate behavior, and enhance game development workflows.

---

## 🚀 Features

- 🔤 **Natural Language Commands** — Control Unity objects using GPT-based text parsing
- 🧠 **OpenAI Integration** — Send and receive prompts via the OpenAI Chat API
- 🧩 **Modular Components** — Easily extend or plug into new Unity scenes
- 🎮 **Real-Time Feedback** — Apply AI-generated changes (e.g., color, behavior) live in the Unity editor

---

## 🧰 Tech Stack

- **Unity** (2020+)
- **C#**
- **OpenAI API**
- **Newtonsoft.Json** (for JSON handling)

---

## 🛠️ Setup Instructions

1. **Clone this repository:**
   ```bash
   git clone https://github.com/Afamuefuna/Unity-AI-Sandbox.git
   ```

2. **Open in Unity:**
   Open the project folder using your preferred Unity version (2020.3+ recommended).

3. **Install Dependencies:**
   Import Newtonsoft.Json via Unity Package Manager or manually.

4. **Add your OpenAI API Key to a TextAsset file:**
   - Create a new file named `APIKey.txt` inside the `Assets/Resources/` folder.
   - Paste your API key into the file (no extra spaces or new lines).

5. **Play the Scene:**
   - Press Play in Unity.
   - Watch the AI modify objects based on parsed instructions.

---

## ✨ Current Project: GPTColorChange

**GPTColorChange** is a live demo showcasing the use of OpenAI's GPT to interpret natural language commands and dynamically modify the colors of 3D boxes within a Unity scene.

### 🔧 How It Works

The user types or sends a prompt like:

```csharp
SendPromptToChatGPT("change the color of box 1 to red and box 3 to blue");
```

This message is sent to the ChatGPT API, which replies with a structured JSON object containing hex color values:

```json
{
  "box 1": "#FF0000",
  "box 3": "#0000FF"
}
```

Unity then uses this JSON to locate the corresponding GameObjects (by name) and apply the specified colors to their materials.

### ✅ Highlights

- 🎯 **Selective Updates** — Only changes the color of boxes mentioned in the prompt
- 🎨 **Hex Color Support** — Colors are applied using parsed hex values (e.g., `#FF0000`)
- ⚡ **Real-Time Parsing** — Responses are processed instantly and applied in the scene
- 🧩 **Flexible Commands** — Works with partial prompts like `"make box 2 yellow"`
- 🧠 **Natural Language Input** — Intuitive interface for non-technical users

### 📦 Example GameObjects

Make sure your scene contains 3D objects named:

- `box 1`
- `box 2`
- `box 3`

These names are matched exactly when parsing the GPT response.

---

## 🔐 API Key Security

Make sure not to commit your `APIKey.txt` to version control. Add this to your `.gitignore`:

```
/Assets/Resources/APIKey.txt
```

---

## 📌 TODO / Future Ideas

- Voice-to-command using speech recognition
- Object spawning and destruction via prompts
- GPT-generated dialogue systems
- Chatbot-style in-game NPCs
- Multi-object batch operations
- Scene management through natural language
- AI-driven procedural content generation

---

## 🧑‍💻 Author

**Afamuefuna Simon** — [GitHub](https://github.com/Afamuefuna) | [LinkedIn](https://www.linkedin.com/in/simon-afamuefuna-81b764193/)

---

## 📄 License

This project is licensed under the MIT License. See LICENSE for more details.