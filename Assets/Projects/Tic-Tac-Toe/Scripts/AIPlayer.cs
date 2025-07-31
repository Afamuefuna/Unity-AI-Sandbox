using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Handles AI player functionality for Tic-Tac-Toe using OpenAI API.
/// This script communicates with the GameLogic and makes moves for the AI player.
/// </summary>
public class AIPlayer : MonoBehaviour
{
    // --- OPENAI API CONFIGURATION ---
    [Header("OpenAI Configuration")]
    [SerializeField] private string openAIApiKey = "YOUR_API_KEY_HERE"; // Set this in the inspector
    [SerializeField] private string openAIModel = "gpt-3.5-turbo"; // or "gpt-4" if you have access
    [SerializeField] private float aiThinkingDelay = 1f; // Minimum delay before AI makes move (for better UX)
    
    private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";

    // --- EVENTS ---
    public static event Action OnAIThinking;           // Signals AI is processing
    public static event Action OnAIFinished;           // Signals AI finished processing

    // --- REFERENCES ---
    private GameLogic gameLogic;
    
    // --- STATE ---
    private bool isProcessing = false;

    // --- UNITY LIFECYCLE ---
    void Start()
    {
        // Get reference to GameLogic
        gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic == null)
        {
            Debug.LogError("TicTacToeAIPlayer: GameLogic not found!");
            return;
        }

        // Subscribe to game events
        GameLogic.OnMoveMade += OnMoveMade;
        GameLogic.OnGameWon += OnGameEnd;
        GameLogic.OnGameDraw += OnGameEnd;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        GameLogic.OnMoveMade -= OnMoveMade;
        GameLogic.OnGameWon -= OnGameEnd;
        GameLogic.OnGameDraw -= OnGameEnd;
    }

    // --- EVENT HANDLERS ---
    private void OnMoveMade(int index, GameLogic.Player player)
    {
        // If a move was made and it's now AI's turn, request AI move
        if (!gameLogic.IsGameOver() && gameLogic.GetCurrentPlayer() == GameLogic.Player.O && !isProcessing)
        {
            StartCoroutine(RequestAIMove());
        }
    }

    private void OnGameEnd(GameLogic.Player winner = GameLogic.Player.None)
    {
        // Stop any ongoing AI processing when game ends
        isProcessing = false;
    }

    private void OnGameEnd()
    {
        // Overload for draw event
        OnGameEnd(GameLogic.Player.None);
    }

    // --- AI LOGIC ---
    private IEnumerator RequestAIMove()
    {
        if (isProcessing) yield break;
        
        isProcessing = true;
        OnAIThinking?.Invoke();
        Debug.Log("AI is thinking...");

        // Add a small delay for better UX (so AI doesn't seem instant)
        yield return new WaitForSeconds(aiThinkingDelay);

        // Prepare the prompt for OpenAI
        string prompt = CreateAIPrompt();

        // Create the request payload
        string jsonPayload = CreateOpenAIPayload(prompt);

        // Send request to OpenAI
        using (UnityWebRequest request = new UnityWebRequest(OPENAI_API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerText();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIApiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string response = request.downloadHandler.text;
                    int aiMove = ParseAIResponse(response);
                    
                    if (aiMove >= 0 && aiMove < 9)
                    {
                        Debug.Log($"AI chose position: {aiMove}");
                        // Make the move through GameLogic's internal method
                        gameLogic.MakeAIMove(aiMove);
                    }
                    else
                    {
                        Debug.LogError("AI returned invalid move, making random move");
                        MakeRandomMove();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing AI response: {e.Message}");
                    MakeRandomMove();
                }
            }
            else
            {
                Debug.LogError($"OpenAI API request failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                MakeRandomMove();
            }
        }

        isProcessing = false;
        OnAIFinished?.Invoke();
    }

    private string CreateAIPrompt()
    {
        string gameState = GetGameStateString();
        
        string prompt = $@"You are playing Tic-Tac-Toe as player O against a human player X.

{gameState}

Rules:
- You are O, your opponent is X
- Choose an empty position (0-8) to place your O
- Empty positions show their number (0-8)
- Try to win the game, or block your opponent from winning
- Play strategically and intelligently

Respond with ONLY the number (0-8) of the position where you want to place your O. Do not include any explanation or additional text.";

        return prompt;
    }

    private string GetGameStateString()
    {
        GameLogic.Player[] board = gameLogic.GetBoard();
        string state = "Current board state (positions 0-8):\n";
        
        for (int i = 0; i < 9; i++)
        {
            char symbol = board[i] == GameLogic.Player.X ? 'X' : 
                         (board[i] == GameLogic.Player.O ? 'O' : 
                         (char)('0' + i));
            state += symbol;
            
            if (i % 3 == 2) 
                state += "\n";
            else 
                state += " | ";
        }
        
        state += "\nAvailable positions: ";
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == GameLogic.Player.None)
            {
                state += i + " ";
            }
        }
        
        return state;
    }

    private string CreateOpenAIPayload(string prompt)
    {
        // Simple JSON creation - in production, consider using a JSON library
        string escapedPrompt = prompt.Replace("\"", "\\\"").Replace("\n", "\\n");
        
        string payload = $@"{{
            ""model"": ""{openAIModel}"",
            ""messages"": [
                {{
                    ""role"": ""system"",
                    ""content"": ""You are an expert Tic-Tac-Toe player. Always respond with only a single digit (0-8) representing your move choice.""
                }},
                {{
                    ""role"": ""user"",
                    ""content"": ""{escapedPrompt}""
                }}
            ],
            ""max_tokens"": 5,
            ""temperature"": 0.3
        }}";

        return payload;
    }

    private int ParseAIResponse(string response)
    {
        try
        {
            // Parse the JSON response to extract the AI's move
            int contentStart = response.IndexOf("\"content\":\"") + 11;
            if (contentStart <= 10) return -1;
            
            int contentEnd = response.IndexOf("\"", contentStart);
            if (contentEnd <= contentStart) return -1;
            
            string content = response.Substring(contentStart, contentEnd - contentStart);
            Debug.Log($"AI Response Content: '{content}'");
            
            // Extract the first valid digit found in the response
            GameLogic.Player[] board = gameLogic.GetBoard();
            foreach (char c in content)
            {
                if (char.IsDigit(c))
                {
                    int move = int.Parse(c.ToString());
                    if (move >= 0 && move <= 8 && board[move] == GameLogic.Player.None)
                    {
                        return move;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing AI response: {e.Message}");
        }

        return -1; // Invalid move
    }

    private void MakeRandomMove()
    {
        // Fallback: make a random valid move
        GameLogic.Player[] board = gameLogic.GetBoard();
        System.Random random = new System.Random();
        int attempts = 0;
        
        while (attempts < 20) // Prevent infinite loop
        {
            int randomIndex = random.Next(0, 9);
            if (board[randomIndex] == GameLogic.Player.None)
            {
                Debug.Log($"Making fallback random move at position: {randomIndex}");
                gameLogic.MakeAIMove(randomIndex);
                return;
            }
            attempts++;
        }
        
        Debug.LogError("Could not make random move - this shouldn't happen!");
    }

    // --- PUBLIC METHODS ---
    public bool IsProcessing() => isProcessing;
    
    /// <summary>
    /// Force the AI to make a move (for testing purposes)
    /// </summary>
    public void ForceAIMove()
    {
        if (!isProcessing && gameLogic.GetCurrentPlayer() == GameLogic.Player.O && !gameLogic.IsGameOver())
        {
            StartCoroutine(RequestAIMove());
        }
    }
}