using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DrawDash
{
    public class Gameplay : MonoBehaviour
    {
        public TMP_Text drawWordText;
        public TMP_Text countdownText;
        [SerializeField] Button submitButton;
        [SerializeField] Button giveUpButton;
        [SerializeField] Button resetButton;
        public string drawWord;
        public int countdown;
        [SerializeField] GameManager gameManager;

        void Start()
        {
            submitButton.onClick.AddListener(() =>
            {
                gameManager.EndGame();
            });

            giveUpButton.onClick.AddListener(() =>
            {
                gameManager.GiveUp();
            });

            resetButton.onClick.AddListener(() =>
            {
                gameManager.Reset();
            });

            gameObject.SetActive(false);
        }

        public IEnumerator UpdateCountdown()
        {
            while (countdown > 0)
            {
                yield return new WaitForSeconds(1);
                countdown--;
                this.countdownText.text = "Time left: " + countdown;
            }
        }
    }
}
