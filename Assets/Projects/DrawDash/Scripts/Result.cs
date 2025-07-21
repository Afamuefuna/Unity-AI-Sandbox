using System.Collections;
using System.Collections.Generic;
using DrawDash;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DrawDash
{
    public class Result : MonoBehaviour
    {
        public TMP_Text scoreText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private GPTVision gptVision;
        [SerializeField] private GameManager gameManager;

        void Start()
        {
            playAgainButton.onClick.AddListener(() =>
            {
                gameManager.StartGame();
            });

            gameObject.SetActive(false);
        }
    }

}