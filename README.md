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

## ✨ Current Project: GPTCommand

**GPTColorChange** is the main project demonstrating natural language control of Unity objects material colors.

### Example Usage

When calling:
```csharp
SendPromptToChatGPT("change color of box 1 to red and box 3 to blue");
```

The GPT response might return:
```json
{
  "box 1": "#FF0000",
  "box 3": "#0000FF"
}
```

Unity will then automatically color those boxes accordingly.

### Key Features
- Parse natural language commands into actionable Unity operations
- Real-time object manipulation through AI interpretation
- JSON-based response handling for structured data
- Seamless integration with Unity's component system

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