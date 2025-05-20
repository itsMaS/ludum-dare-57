using MarKit;
using MarTools;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEndWindow : MonoBehaviour
{
    [SerializeField] Node rankingNode;

    private void Awake()
    {
        gameObject.SetActive(false);
        rankingNode.gameObject.SetActive(false);

        //LeaderboardManager.Instance.OnLeaderboardUpdate.AddListener(Load);
        GameManager.Instance.OnGameOver.AddListener(OpenEndWindow);
    }

    private void OpenEndWindow()
    {
        gameObject.SetActive(true);
    }

    private void Load(List<LeaderboardManager.Ranking> arg0)
    {
        int playerIndex = arg0.FindIndex(x => x.isYou);

        List<LeaderboardManager.Ranking> Shown = new List<LeaderboardManager.Ranking>();
        Shown.Add(arg0[playerIndex]);

        int range = 2;

        for (int i = 1; i <= range; i++)
        {
            if(playerIndex - i >= 0)
                Shown.Insert(0,(arg0[playerIndex - i]));
            
            if(playerIndex + i < arg0.Count)
                Shown.Add(arg0[playerIndex + i]);
        }

        rankingNode.Populate(Shown, (el, index, node) =>
        {
            var tween = node.GetComponent<TweenCore>();

            tween.delayRange = index * 0.2f * Vector2.one;
            tween.PlayForwards();

            node.Texts[0].SetText($"{el.Rank}.");
            node.Texts[1].SetText($"{el.Name} {(el.isYou ? "<color=red>(You)" : "")}");
            node.Texts[2].SetText($"{el.Score}");
        });
    }
}
