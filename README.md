# Unity-AI-Sandbox

Unity-AI-Sandbox is a collection of experimental projects that explore the integration of Artificial Intelligence (AI) with the Unity game engine. It includes prototypes demonstrating how AI models like OpenAI's GPT can be used to drive interactivity, generate behavior, and enhance game development workflows.

-----

## 🚀 Features

  - 🔤 **Natural Language Commands** — Control Unity objects using GPT-based text parsing.
  - 🧠 **OpenAI Integration** — Send and receive prompts via the OpenAI Chat API.
  - 🧩 **Modular Components** — Easily extend or plug into new Unity scenes.
  - 🎮 **Real-Time Feedback** — Apply AI-generated changes (e.g., color, behavior) live in the Unity editor.

-----

## 🧰 Tech Stack

  - Unity (2020+)
  - C\#
  - OpenAI API
  - Newtonsoft.Json (for JSON handling)

-----

## 📦 Third-Party Assets & Plugins

  - **Kenney Assets** — Used for various art assets.
  - **UiRoundedCorners** — A plugin for creating rounded UI elements.

-----

## 🛠️ Setup Instructions

1.  **Clone this repository:**
    ```bash
    git clone https://github.com/Afamuefuna/Unity-AI-Sandbox.git
    ```
2.  **Open in Unity:**
    Open the project folder using your preferred Unity version (2020.3+ recommended).
3.  **Install Dependencies:**
    Import Newtonsoft.Json via Unity Package Manager or manually.
4.  **Add your OpenAI API Key to a TextAsset file:**
      * Create a new file named `APIKey.txt` inside the `Assets/Resources/` folder.
      * Paste your API key into the file (no extra spaces or new lines).
5.  **Play the Scene:**
      * Press Play in Unity.
      * Watch the AI modify objects based on parsed instructions.

-----

## ✨ Current Projects

### 🎮 Draw Dash

Draw Dash is a mini-game where the AI gives you a random drawing challenge and a countdown timer. After the timer runs out (or you submit early), the AI analyzes your drawing and gives you a score.

#### 🔧 How It Works

1.  The player receives a prompt from GPT like:
    ```arduino
    "Draw a rocket ship"
    ```
2.  A countdown timer starts (e.g., 30 seconds).
3.  The player draws on the screen.
4.  When the timer ends or the player submits, the drawing is sent to the OpenAI Vision API for analysis and scoring.
5.  The player receives feedback and a score, then can replay with a new drawing challenge.

#### ✅ Highlights

  - ✏️ **Drawing Canvas** — Players draw freely on a 2D surface.
  - ⏱️ **Timed Sessions** — Encourages quick creativity.
  - 🤖 **AI Evaluation** — Uses GPT-4 Vision to evaluate and respond to the drawing.
  - 🔁 **Replay Loop** — New prompt every round for ongoing play.

-----

### 🎨 GPTColorChange

GPTColorChange is a live demo showcasing the use of OpenAI's GPT to interpret natural language commands and dynamically modify the colors of 3D boxes within a Unity scene.

#### 🔧 How It Works

1.  The user types or sends a prompt like:
    ```csharp
    SendPromptToChatGPT("change the color of box 1 to red and box 3 to blue");
    ```
2.  This message is sent to the ChatGPT API, which replies with a structured JSON object:
    ```json
    {
      "box 1": "#FF0000",
      "box 3": "#0000FF"
    }
    ```
3.  Unity locates the corresponding GameObjects and applies the specified colors.

#### ✅ Highlights

  - 🎯 **Selective Updates**
  - 🎨 **Hex Color Support**
  - ⚡ **Real-Time Parsing**
  - 🧩 **Flexible Commands**
  - 🧠 **Natural Language Input**

-----

## 🔐 API Key Security

Make sure not to commit your `APIKey.txt` to version control. Add this to your `.gitignore`:

```swift
/Assets/Resources/APIKey.txt
```

-----

## 📌 TODO / Future Ideas

  - Voice-to-command using speech recognition
  - Object spawning and destruction via prompts
  - GPT-generated dialogue systems
  - Chatbot-style in-game NPCs
  - Multi-object batch operations
  - Scene management through natural language
  - AI-driven procedural content generation
  - Drawing assistance / AI sketch correction

-----

## 🧑‍💻 Author

**Afamuefuna Simon** — [GitHub](https://github.com/Afamuefuna) | [LinkedIn](https://www.google.com/search?q=https://www.linkedin.com/in/afamuefuna-simon-1a2b3c4d5/)

-----

## 📄 License

This project is licensed under the MIT License. See `LICENSE` for more details.