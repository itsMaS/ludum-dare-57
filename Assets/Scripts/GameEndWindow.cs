using MarKit;
using MarTools;
using Steamworks;
using System;
using UnityEngine;

public class GameEndWindow : MonoBehaviour
{
    [SerializeField] Node rankingNode;

    private void Awake()
    {
        GameManager.Instance.OnGameOver.AddListener(GameOver);
        GameManager.Instance.OnRankingsLoaded.AddListener(RankingsLoaded);

        gameObject.SetActive(false);
        rankingNode.gameObject.SetActive(false);
    }

    private void RankingsLoaded(LeaderboardData arg0)
    {
        rankingNode.Populate(arg0.entries, (el, index, node) =>
        {
            node.Texts[0].SetText($"{el.GlobalRank}.");
            node.Texts[1].SetText($"{el.User.Name} {(SteamClient.SteamId == el.User.Id ? "<color=red>(You)" : "")}");
            node.Texts[2].SetText($"{el.Score}");
        });
    }

    private void GameOver()
    {
        gameObject.SetActive(true);
        rankingNode.Clear();
    }
}
