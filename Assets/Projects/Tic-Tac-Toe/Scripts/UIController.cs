using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI; // Required to use a List

/// <summary>
/// This script acts as the bridge between the UI elements and the game logic.
/// It handles button clicks and updates the UI based on game state changes.
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Game Logic")]
    [Tooltip("Drag the GameObject that has the TicTacToeGame script here.")]
    public GameLogic gameLogic;

    [Header("UI Elements")]
    [Tooltip("Drag all 9 of your board buttons here, in order from 0 to 8.")]
    public List<Button> boardButtons;

    [Tooltip("Optional: A Text element to display game status like 'Player X Wins!'")]
    public TMP_Text statusText;
    
    [Tooltip("Optional: A button to restart the game.")]
    public Button restartButton;

    public Sprite playerOSprite, playerXSprite;

    // Subscribe to events when the script is enabled
    void OnEnable()
    {
        GameLogic.OnMoveMade += HandleMoveMade;
        GameLogic.OnGameWon += HandleGameWon;
        GameLogic.OnGameDraw += HandleGameDraw;
    }

    // Unsubscribe from events when the script is disabled to prevent errors
    void OnDisable()
    {
        GameLogic.OnMoveMade -= HandleMoveMade;
        GameLogic.OnGameWon -= HandleGameWon;
        GameLogic.OnGameDraw -= HandleGameDraw;
    }

    void Start()
    {
        // Add a listener to each board button
        for (int i = 0; i < boardButtons.Count; i++)
        {
            int index = i; // Store the index locally for the button's click event
            boardButtons[i].onClick.AddListener(() => OnBoardButtonClick(index));
        }
        
        // Add a listener to the restart button if it exists
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // Set up the initial UI state
        ResetUI();
    }

    // Called when a board button is clicked
    private void OnBoardButtonClick(int index)
    {
        // Tell the game logic to make a move at this button's index
        gameLogic.MakeMove(index);
    }

    // --- EVENT HANDLERS (These methods are called by the game logic) ---

    // Updates the UI when a move is successfully made
    private void HandleMoveMade(int index, GameLogic.Player player)
    {
        // Get the Text component of the button that was clicked
        Text buttonText = boardButtons[index].GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = player.ToString(); // Set text to "X" or "O"
            buttonText.fontSize = 100; // Make it look nice
        }
        boardButtons[index].image.sprite = player == GameLogic.Player.O ? playerOSprite : playerXSprite;
        boardButtons[index].interactable = false; // Disable the button
    }

    // Updates the UI when a player wins
    private void HandleGameWon(GameLogic.Player winner)
    {
        if (statusText != null)
        {
            statusText.text = $"Player {winner} Wins!";
        }
        DisableAllBoardButtons();
    }

    // Updates the UI when the game is a draw
    private void HandleGameDraw()
    {
        if (statusText != null)
        {
            statusText.text = "It's a Draw!";
        }
        DisableAllBoardButtons();
    }
    
    // --- UI MANAGEMENT ---

    private void RestartGame()
    {
        gameLogic.StartNewGame();
        ResetUI();
    }

    private void ResetUI()
    {
        // Re-enable all buttons and clear their text
        foreach (var button in boardButtons)
        {
            button.interactable = true;
            button.image.sprite = null;
        }

        // Clear the status text
        if (statusText != null)
        {
            statusText.text = "Player X's Turn"; // Or just ""
        }
    }

    private void DisableAllBoardButtons()
    {
        foreach (var button in boardButtons)
        {
            button.interactable = false;
        }
    }
}