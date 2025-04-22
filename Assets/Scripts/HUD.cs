using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarKit
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] GameObject livesExample;

        PlayerController player;

        List<GameObject> LivesIndicators = new List<GameObject>();

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
    }
}
