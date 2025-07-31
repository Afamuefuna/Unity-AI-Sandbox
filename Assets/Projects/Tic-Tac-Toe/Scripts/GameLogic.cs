using UnityEngine;
using System; // Required for using Actions (events)

/// <summary>
/// Handles the core logic for a Tic-Tac-Toe game.
/// This class is responsible for the game state, player turns, and win/draw conditions.
/// It does not handle any UI or player input directly.
/// </summary>
public class GameLogic : MonoBehaviour
{
    /// <summary>
    /// Represents the players in the game.
    /// </summary>
    public enum Player { None, X, O }

    // --- EVENTS ---
    // Events that other scripts (like a UI manager) can subscribe to.
    public static event Action<int, Player> OnMoveMade; // Sends the board index and player who made the move.
    public static event Action<Player> OnGameWon;      // Sends the winning player.
    public static event Action OnGameDraw;             // Signals a draw game.

    // --- GAME STATE ---
    private Player[] board;
    private Player currentPlayer;
    private bool isGameOver;

    // --- UNITY LIFECYCLE ---
    void Start()
    {
        StartNewGame();
    }

    // --- PUBLIC METHODS ---

    /// <summary>
    /// Starts or restarts the game, resetting the board and player turn.
    /// </summary>
    public void StartNewGame()
    {
        board = new Player[9]; // Represents the 3x3 grid
        currentPlayer = Player.X; // X always starts
        isGameOver = false;

        Debug.Log("--- New Game Started ---");
        Debug.Log("Player X's turn.");
    }

    /// <summary>
    /// Attempts to place the current player's mark at the specified board index.
    /// </summary>
    /// <param name="index">The board index (0-8) where the player wants to move.</param>
    /// <returns>True if the move was successful, false otherwise.</returns>
    public bool MakeMove(int index)
    {
        // Validate the move
        if (isGameOver || index < 0 || index >= board.Length || board[index] != Player.None)
        {
            Debug.LogWarning($"Invalid Move: Cell {index} is not available or game is over.");
            return false;
        }

        // Apply the move
        board[index] = currentPlayer;
        Debug.Log($"Player {currentPlayer} placed a mark at index {index}.");
        OnMoveMade?.Invoke(index, currentPlayer);

        // Check for game end conditions
        if (CheckForWin())
        {
            isGameOver = true;
            Debug.Log($"GAME OVER! Player {currentPlayer} wins!");
            OnGameWon?.Invoke(currentPlayer);
        }
        else if (CheckForDraw())
        {
            isGameOver = true;
            Debug.Log("GAME OVER! It's a draw!");
            OnGameDraw?.Invoke();
        }
        else
        {
            // If the game is not over, switch to the next player
            SwitchPlayer();
        }

        return true;
    }

    /// <summary>
    /// Overload for MakeMove that accepts row and column.
    /// </summary>
    public bool MakeMove(int row, int col)
    {
        if (row < 0 || row > 2 || col < 0 || col > 2) return false;
        int index = row * 3 + col;
        return MakeMove(index);
    }


    // --- PRIVATE HELPER METHODS ---

    private void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == Player.X) ? Player.O : Player.X;
        Debug.Log($"It's now Player {currentPlayer}'s turn.");
    }

    private bool CheckForWin()
    {
        // All 8 possible winning combinations
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

            Debug.Log($"Checking win condition: {a}, {b}, {c}");

            // Check if the three cells have the same, non-empty player mark
            if (board[a] != Player.None && board[a] == board[b] && board[a] == board[c])
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckForDraw()
    {
        // If any cell is still empty, it's not a draw yet
        foreach (Player cell in board)
        {
            if (cell == Player.None)
            {
                return false;
            }
        }
        // If all cells are filled and CheckForWin() was false, it's a draw
        return true;
    }
}