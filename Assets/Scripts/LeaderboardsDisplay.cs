using MarTools;
using System;
using UnityEngine;
using UnityEngine.Events;

public class LeaderboardsDisplay : MonoBehaviour
{
    public UnityEvent OnOpened;
    public UnityEvent OnFailedLoad;
    public UnityEvent OnLoaded;
    public UnityEvent OnRefresh;

    public Node rankingNode;

    private void OnEnable()
    {
        //rankingNode.Clear();
        OnOpened.Invoke();

        Debug.Log("Opened");
        LeaderboardManager.Instance.RequestLeaderboard(LeaderboardsLoaded);
    }

    private void Awake()
    {
        LeaderboardManager.Instance.OnFetched.AddListener(LeaderboardsLoaded);
    }

    private void LeaderboardsLoaded(LeaderboardManager.LeaderboardResult obj)
    {
        if(obj == null)
        {
            OnFailedLoad.Invoke();
            Debug.Log("Failed load");
        }
        else
        {
            OnLoaded.Invoke();
            Debug.Log($"Loaded {obj.Rankings.Count}");

            rankingNode.Populate(obj.Rankings, (el, i, node) =>
            {
                node.Texts[0].SetText($"{el.Rank}.");
                node.Texts[1].SetText($"{el.Name}");
                node.Texts[2].SetText($"{el.Score}");
            });
        }
    }

    public void Refresh()
    {
        OnRefresh.Invoke();
        //rankingNode.Clear();
        LeaderboardManager.Instance.RequestLeaderboard(LeaderboardsLoaded);
    }
}
