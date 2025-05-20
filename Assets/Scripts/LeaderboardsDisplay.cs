using MarTools;
using System;
using UnityEngine;
using UnityEngine.Events;

public class LeaderboardsDisplay : MonoBehaviour
{
    public UnityEvent OnLeaderboardsLoaded;
    public Node rankingNode;

    private void OnEnable()
    {
        LeaderboardManager.Instance.RequestLeaderboard(LeaderboardsLoaded);
    }

    private void LeaderboardsLoaded(LeaderboardManager.LeaderboardResult obj)
    {
        OnLeaderboardsLoaded.Invoke();

        rankingNode.Populate(obj.Rankings, (el, i, node) =>
        {
            node.Texts[0].SetText($"{el.Rank}.");
            node.Texts[1].SetText($"{el.Name}");
            node.Texts[2].SetText($"{el.Score}");
        });
    }
}
