using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Home : MonoBehaviour
{
    [SerializeField] Button startGameButton;
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        startGameButton.onClick.AddListener(()=>
        {
            gameManager.StartGame();
        });
    }
}
