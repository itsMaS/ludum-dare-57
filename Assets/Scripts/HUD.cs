using MarTools;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarKit
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] GameObject livesExample;
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI highscoreText;
        [SerializeField] Image comboFillImage;
        [SerializeField] TextMeshProUGUI comboAmountText;

        [SerializeField]
        GroupBehavior[] ComboEffects;

        PlayerController player;

        List<GameObject> LivesIndicators = new List<GameObject>();

        float scoreTarget;
        float currentScore;
        float scoreVelocity;


        private void Start()
        {
            player = FindObjectOfType<PlayerController>();

            for (int i = 0; i < player.maxLives; i++)
            {
                var indicator = Instantiate(livesExample.gameObject, livesExample.transform.parent);
                LivesIndicators.Add(indicator);

                indicator.gameObject.SetActive(true);
            }
            livesExample.gameObject.SetActive(false);

            player.OnTakeDamage.AddListener(UpdateLives);


            UpdateLives();

            GameManager.Instance.OnRestart.AddListener(Restart);
            GameManager.Instance.OnScoreChanged.AddListener(ScoreChanged);

            GameManager.Instance.OnComboGained.AddListener(UpdateCombo);
            GameManager.Instance.OnComboLost.AddListener(UpdateCombo);

            UpdateHighscore();
            scoreText.SetText("0");


            UpdateCombo();
        }

        private void Restart()
        {
            currentScore = 0;
            scoreTarget = 0;
            scoreText.SetText("0");

            UpdateLives();
            UpdateCombo();
        }

        private void UpdateCombo()
        {
            int comboIndex = GameManager.Instance.comboLevel -1;

            comboIndex = Math.Clamp(comboIndex, 0, ComboEffects.Length-1);

            ComboEffects[comboIndex].Activate();
        }

        private void ScoreChanged(int arg0)
        {
            UpdateHighscore();
            scoreTarget = arg0;
        }

        private void UpdateHighscore()
        {
            highscoreText.SetText(GameManager.highscore.ToString());
        }

        private void UpdateLives()
        {
            for (int i = 0; i < player.maxLives; i++)
            {
                var indicator = LivesIndicators[i];

                // Alive
                if (i < player.currentLives)
                {
                    indicator.transform.GetChild(0).gameObject.SetActive(true);
                    indicator.transform.GetChild(1).gameObject.SetActive(false);
                }
                // Dead
                else
                {
                    indicator.transform.GetChild(0).gameObject.SetActive(false);
                    indicator.transform.GetChild(1).gameObject.SetActive(true);
                }

            }
        }

        private void Update()
        {
            currentScore = Mathf.SmoothDamp(currentScore, scoreTarget, ref scoreVelocity, 0.2f, 999, Time.deltaTime);
            scoreText.SetText(Mathf.RoundToInt(currentScore).ToString());

            comboFillImage.fillAmount = GameManager.Instance.comboProgress;

            comboAmountText.SetText($"x{GameManager.Instance.comboLevel}");
        }
    }
}
