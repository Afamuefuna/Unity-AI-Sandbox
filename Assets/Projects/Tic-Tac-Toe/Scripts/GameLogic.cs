using UnityEngine;
using System;

public class GameLogic : MonoBehaviour
{
    public enum Player { None, X, O }
    public static event Action<int, int, int> OnWinningLine;
    public static event Action<int, Player> OnMoveMade; // Sends the board index and player who made the move.
    public static event Action<Player, int[]> OnGameWon; // Sends the winning player and winning line indices.
    public static event Action OnGameDraw;             // Signals a draw game.

    private Player[] board;
    private Player currentPlayer;
    private bool isGameOver;
    private int[] winningLine; // Store the indices of the winning line

    public void StartNewGame()
    {
        board = new Player[9]; // Represents the 3x3 grid
        currentPlayer = Player.X; // X always starts
        isGameOver = false;
        winningLine = null;

        Debug.Log("--- New Game Started ---");
        Debug.Log("Player X's turn.");
    }

    public bool MakeMove(int index)
    {
        return ProcessMove(index, currentPlayer);
    }

    public bool MakeMove(int row, int col)
    {
        if (row < 0 || row > 2 || col < 0 || col > 2) return false;
        int index = row * 3 + col;
        return MakeMove(index);
    }

    public bool MakeAIMove(int index)
    {
        if (currentPlayer != Player.O)
        {
            Debug.LogWarning("MakeAIMove can only be called when it's Player O's turn.");
            return false;
        }
        return ProcessMove(index, Player.O);
    }

    public Player[] GetBoard()
    {
        return (Player[])board.Clone();
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void SetGameOver(bool value)
    {
        isGameOver = value;
    }

    public int[] GetWinningLine()
    {
        return winningLine;
    }

    public string GetGameStateString()
    {
        string state = "Current board state (0-8 positions):\n";
        for (int i = 0; i < 9; i++)
        {
            char symbol = board[i] == Player.X ? 'X' : (board[i] == Player.O ? 'O' : (char)('0' + i));
            state += symbol;
            if (i % 3 == 2) state += "\n";
            else state += " | ";
        }
        return state;
    }

    private bool ProcessMove(int index, Player player)
    {
        if (isGameOver || index < 0 || index >= board.Length || board[index] != Player.None)
        {
            Debug.LogWarning($"Invalid Move: Cell {index} is not available or game is over.");
            return false;
        }

        if (player != currentPlayer)
        {
            Debug.LogWarning($"It's not Player {player}'s turn. Current player is {currentPlayer}.");
            return false;
        }

        board[index] = currentPlayer;
        Debug.Log($"Player {currentPlayer} placed a mark at index {index}.");
        OnMoveMade?.Invoke(index, currentPlayer);

        if (CheckForWin())
        {
            isGameOver = true;
            Debug.Log($"GAME OVER! Player {currentPlayer} wins!");
            OnGameWon?.Invoke(currentPlayer, winningLine);
        }
        else if (CheckForDraw())
        {
            isGameOver = true;
            Debug.Log("GAME OVER! It's a draw!");
            OnGameDraw?.Invoke();
        }
        else
        {
            SwitchPlayer();
        }

        return true;
    }

    private void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == Player.X) ? Player.O : Player.X;
        Debug.Log($"It's now Player {currentPlayer}'s turn.");
    }

    private bool CheckForWin()
    {
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

            if (board[a] != Player.None && board[a] == board[b] && board[a] == board[c])
            {
                winningLine = new int[] { a, b, c };
                OnWinningLine?.Invoke(a, b, c); // Fire event with winning indices
                return true;
            }
        }
        return false;
    }

    private bool CheckForDraw()
    {
        foreach (Player cell in board)
        {
            if (cell == Player.None)
            {
                return false;
            }
        }
        return true;
    }
}