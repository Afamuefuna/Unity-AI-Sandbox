using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    Home home;
    GameLogic gameLogic;

    public void Start()
    {
        home = FindAnyObjectByType<Home>();
        gameLogic = FindObjectOfType<GameLogic>();
    }

    public void StartGame()
    {
        home.gameObject.SetActive(false);
        gameLogic.StartNewGame();
    }

    public void QuitPlay()
    {
        gameLogic.SetGameOver(true);
        home.gameObject.SetActive(true);
    }
}
