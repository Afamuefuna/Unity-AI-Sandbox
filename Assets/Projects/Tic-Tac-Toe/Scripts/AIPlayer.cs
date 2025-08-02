using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class AIPlayer : MonoBehaviour
{
    [SerializeField] private string apiKey = "YOUR_API_KEY_HERE"; 
    [SerializeField] private string openAIModel = "gpt-4o"; 
    [SerializeField] private float aiThinkingDelay = 1f; 
    private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";

    public static event Action OnAIThinking;          
    public static event Action OnAIFinished;           

    private GameLogic gameLogic;

    private bool isProcessing = false;

    void Start()
    {
        gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic == null)
        {
            Debug.LogError("TicTacToeAIPlayer: GameLogic not found!");
            return;
        }

        GameLogic.OnMoveMade += OnMoveMade;
        GameLogic.OnGameWon += OnGameEnd;
        GameLogic.OnGameDraw += OnGameEnd;

        TextAsset apiKeyAsset = Resources.Load<TextAsset>("APIKey");
        apiKey = apiKeyAsset.text;
    }

    void OnDestroy()
    {
        GameLogic.OnMoveMade -= OnMoveMade;
        GameLogic.OnGameWon -= OnGameEnd;
        GameLogic.OnGameDraw -= OnGameEnd;
    }

    private async void OnMoveMade(int index, GameLogic.Player player)
    {
        await Task.Yield();

        if (!gameLogic.IsGameOver() && gameLogic.GetCurrentPlayer() == GameLogic.Player.O && !isProcessing)
        {
            StartCoroutine(RequestAIMove());
        }
    }

    private void OnGameEnd(GameLogic.Player winner = GameLogic.Player.None, int[] winningLine = null)
    {
        isProcessing = false;
    }

    private void OnGameEnd()
    {
        OnGameEnd(GameLogic.Player.None);
    }

    private IEnumerator RequestAIMove()
    {
        if (isProcessing) yield break;

        isProcessing = true;
        OnAIThinking?.Invoke();

        yield return new WaitForSeconds(aiThinkingDelay);

        string prompt = CreateAIPrompt();

        string jsonPayload = CreateOpenAIPayload(prompt);

        using (UnityWebRequest request = new UnityWebRequest(OPENAI_API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string response = request.downloadHandler.text;
                    int aiMove = Utility.ExtractContentJSONInt(response);

                    Debug.Log($"AI Response: {response}");
                    Debug.Log($"AI Move: {aiMove}");

                    if (aiMove >= 0 && aiMove < 9)
                    {
                        Debug.Log($"AI chose position: {aiMove}");
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

{gameState}";

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

    state += "\n";
    for (int i = 0; i < 9; i++)
    {
        if (board[i] == GameLogic.Player.None)
        {
            state += $"Position {i} is empty.\n";
        }
    }

    int[,] winConditions = new int[,]
    {
        {0, 1, 2}, {3, 4, 5}, {6, 7, 8}, // Rows
        {0, 3, 6}, {1, 4, 7}, {2, 5, 8}, // Columns
        {0, 4, 8}, {2, 4, 6}             // Diagonals
    };

    for (int i = 0; i < winConditions.GetLength(0); i++)
    {
        int a = winConditions[i, 0];
        int b = winConditions[i, 1];
        int c = winConditions[i, 2];

        GameLogic.Player p1 = board[a];
        GameLogic.Player p2 = board[b];
        GameLogic.Player p3 = board[c];

        // Check for 2 O's and an empty — opportunity to win
        if ((p1 == GameLogic.Player.O && p2 == GameLogic.Player.O && board[c] == GameLogic.Player.None))
            state += $"you the AI O can win by playing at position {c} (line: {a}, {b}, {c}).\n";
        else if ((p1 == GameLogic.Player.O && board[b] == GameLogic.Player.None && p3 == GameLogic.Player.O))
            state += $"you the AI O can win by playing at position {b} (line: {a}, {b}, {c}).\n";
        else if ((board[a] == GameLogic.Player.None && p2 == GameLogic.Player.O && p3 == GameLogic.Player.O))
            state += $"you the AI O can win by playing at position {a} (line: {a}, {b}, {c}).\n";

        // Check for 2 X's and an empty — need to block
        if ((p1 == GameLogic.Player.X && p2 == GameLogic.Player.X && board[c] == GameLogic.Player.None))
            state += $"Block X from winning at position {c} (line: {a}, {b}, {c}).\n";
        else if ((p1 == GameLogic.Player.X && board[b] == GameLogic.Player.None && p3 == GameLogic.Player.X))
            state += $"Block X from winning at position {b} (line: {a}, {b}, {c}).\n";
        else if ((board[a] == GameLogic.Player.None && p2 == GameLogic.Player.X && p3 == GameLogic.Player.X))
            state += $"Block X from winning at position {a} (line: {a}, {b}, {c}).\n";
    }

    Debug.Log(state);
    return state;
}



    private string CreateOpenAIPayload(string prompt)
    {
        string escapedPrompt = prompt.Replace("\"", "\\\"").Replace("\n", "\\n");

        string payload = $@"{{
            ""model"": ""{openAIModel}"",
            ""messages"": [
                {{
                    ""role"": ""system"",
                    ""content"": ""Rules:\n- You are O, your opponent is X\n- If you can win in one move, do it\n- Otherwise, block your opponent if they can win next\n- Always choose an empty position (0–8)\n- Play intelligently to win or force a draw\n- The board will show:\n  - X or O for taken spots\n  - 0–8 for empty spots\n- Respond with only the number (0–8) of the position to place your O — nothing else""
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

    private void MakeRandomMove()
    {
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

    public bool IsProcessing() => isProcessing;

    public void ForceAIMove()
    {
        if (!isProcessing && gameLogic.GetCurrentPlayer() == GameLogic.Player.O && !gameLogic.IsGameOver())
        {
            StartCoroutine(RequestAIMove());
        }
    }
}