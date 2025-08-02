using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameLogic gameLogic;
    public List<Button> boardButtons;
    public TMP_Text statusText;
    public Button restartButton;
    public Button quitButton;
    public Sprite playerOSprite, playerXSprite;
    public RectTransform winningLine;
    GameManager gameManager;

    void OnEnable()
    {
        GameLogic.OnMoveMade += HandleMoveMade;
        GameLogic.OnGameWon += HandleGameWon;
        GameLogic.OnGameDraw += HandleGameDraw;
        AIPlayer.OnAIThinking += HandleAIThinking;
        GameLogic.OnWinningLine += DrawWinningLine;
    }

    void OnDisable()
    {
        GameLogic.OnMoveMade -= HandleMoveMade;
        GameLogic.OnGameWon -= HandleGameWon;
        GameLogic.OnGameDraw -= HandleGameDraw;
        AIPlayer.OnAIThinking -= HandleAIThinking;
        GameLogic.OnWinningLine -= DrawWinningLine;
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        for (int i = 0; i < boardButtons.Count; i++)
        {
            int index = i;
            boardButtons[i].onClick.AddListener(() => OnBoardButtonClick(index));
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitPlay);
        }

        ResetUI();
    }

    private void DrawWinningLine(int a, int b, int c)
    {
        Vector3 posA = boardButtons[a].transform.position;
        Vector3 posC = boardButtons[c].transform.position;

        Vector3 midPoint = (posA + posC) / 2f;
        winningLine.position = midPoint;

        float distance = Vector3.Distance(posA, posC);
        winningLine.sizeDelta = new Vector2(distance, winningLine.sizeDelta.y);

        Vector3 direction = (posC - posA).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        winningLine.rotation = Quaternion.Euler(0, 0, angle);
        winningLine.gameObject.SetActive(true);
    }

    private void HandleAIThinking()
    {
        if (statusText != null)
        {
            statusText.text = "AI's Thinking...";
        }
    }

    private void OnBoardButtonClick(int index)
    {
        if (gameLogic.GetCurrentPlayer() == GameLogic.Player.X)
        {
            gameLogic.MakeMove(index);
        }
    }

    private void HandleMoveMade(int index, GameLogic.Player player)
    {
        Text buttonText = boardButtons[index].GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = player.ToString();
            buttonText.fontSize = 100;
        }
        boardButtons[index].image.sprite = player == GameLogic.Player.O ? playerOSprite : playerXSprite;
        boardButtons[index].interactable = false;

        if (statusText != null)
        {
            if (player == GameLogic.Player.O)
            {
                statusText.text = "Your Turn";
            }
            else
            {
                statusText.text = "AI's Turn";
            }
        }
    }

    private void HandleGameWon(GameLogic.Player winner, int[] winningLine)
    {
        if (statusText != null)
        {
            if (winner == GameLogic.Player.O)
            {
                statusText.text = "AI Wins!";
            }
            else
            {
                statusText.text = "You Win!";
            }
        }

        DisableAllBoardButtons();
    }

    private void HandleGameDraw()
    {
        if (statusText != null)
        {
            statusText.text = "It's a Draw!";
        }
        DisableAllBoardButtons();
    }

    private void RestartGame()
    {
        gameLogic.StartNewGame();
        ResetUI();
    }

    private void QuitPlay()
    {
        gameManager.QuitPlay();
    }

    private void ResetUI()
    {
        foreach (var button in boardButtons)
        {
            button.interactable = true;
            button.image.sprite = null;
        }

        if (statusText != null)
        {
            statusText.text = "Your Turn";
        }

        if (winningLine != null)
        {
            winningLine.gameObject.SetActive(false);
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