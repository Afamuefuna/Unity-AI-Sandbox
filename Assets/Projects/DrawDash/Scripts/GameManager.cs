using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DrawDash
{
    public class GameManager : MonoBehaviour
    {
        GPTVision gptVision;
        GPTCommand gptCommand;
        SimpleDraw simpleDraw;
        Home home;
        Gameplay gameplay;
        Result result;
        Load load;

        Coroutine gameplayUIUpdateCountdown;

        void Start()
        {
            gptVision = FindAnyObjectByType<GPTVision>();
            simpleDraw = FindAnyObjectByType<SimpleDraw>();
            home = FindAnyObjectByType<Home>();
            gameplay = FindAnyObjectByType<Gameplay>();
            result = FindAnyObjectByType<Result>();
            gptCommand = FindAnyObjectByType<GPTCommand>();
            load = FindAnyObjectByType<Load>();
        }

        public void StartGame()
        {
            load.Show();
            
            StartCoroutine(gptCommand.GetSketchPrompt(
                onSuccess: (prompt, countdown) =>
                {
                    gameplay.gameObject.SetActive(true);
                    simpleDraw.ClearTexture();

                    gameplay.drawWord = prompt;
                    gameplay.drawWordText.text = "Draw " + gameplay.drawWord;
                    gameplay.countdown = countdown;
                    gameplay.countdownText.text = "Time left: " + countdown;

                    gameplayUIUpdateCountdown = StartCoroutine(gameplay.UpdateCountdown());

                    home.gameObject.SetActive(false);
                    result.gameObject.SetActive(false);
                    load.Hide();
                },
                onError: (error) =>
                {
                    Debug.LogError("Error fetching sketch prompt: " + error);
                    load.Hide();
                }
            ));
        }

        public void GiveUp()
        {
            simpleDraw.ClearTexture();
            StopCoroutine(gameplayUIUpdateCountdown);
            gameplay.gameObject.SetActive(false);
            home.gameObject.SetActive(true);
        }

        public void EndGame()
        {
            StopCoroutine(gameplayUIUpdateCountdown);
            
            load.Show();
            
            StartCoroutine(gptVision.SendImageToOpenAI(simpleDraw.SaveTexture(), gameplay.drawWord, onSuccess: (result) =>
            {
                this.result.gameObject.SetActive(true);
                this.result.scoreText.text =  result;
                if(int.Parse(result) >= 5)
                {
                    this.result.scoreText.color = new Color32(155, 255, 134, 255); // Green
                }else
                {
                    this.result.scoreText.color = new Color32(255, 142, 134, 255); // Red
                }
                load.Hide();
            }, onError: (error) =>
            {
                Debug.LogError("Error sending image to OpenAI: " + error);
                load.Hide();
            }));
        }

        public void Reset()
        {
            simpleDraw.ClearTexture();
        }
    }
}
