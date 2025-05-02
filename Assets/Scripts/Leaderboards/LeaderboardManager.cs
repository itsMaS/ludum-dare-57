using UnityEngine;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using MarTools;
using UnityEngine.Events;
using MarKit;
using System.Linq;

public class LeaderboardManager : Singleton<LeaderboardManager>
{
    [System.Serializable]
    public class Ranking
    {
        public string Name { get; private set; }
        public int Score { get; private set; }
        public int Rank { get; set; }

        public bool isYou { get; private set; }

        public Ranking(LeaderboardEntry entry)
        {
            Score = entry.Score;
            Name = entry.User.Name;

            isYou = entry.User.Id == SteamClient.SteamId;
        }

        public Ranking(int order)
        {
            UnityEngine.Random.InitState(order);

            string[] Words = new string[] {"Cat","Dog","Dragon","Milk","Thing","Scope","All","Time","Sec","Good","John"};
            Name = $"{Words.PickRandom()}{Words.PickRandom()}{Words.PickRandom()}{Random.Range(4989,8979)}";

            Score = order * 10 + Random.Range(1, 200);

            isYou = false;
        }
    }

    public UnityEvent<List<Ranking>> OnLeaderboardUpdate;

    Leaderboard? globalLeaderboard;

    private List<Ranking> Rankings = new List<Ranking>();

    private void Start()
    {
        GameManager.Instance.OnGameOver.AddListener(GameOver);
        FetchLeaderboards();
    }

    private async void GameOver()
    {
        if(globalLeaderboard.HasValue)
        {
            await globalLeaderboard.Value.SubmitScoreAsync(GameManager.Instance.currentScore);
            FetchLeaderboards();
        }
    }

    public async void FetchLeaderboards()
    {
        Debug.Log("Leaderboard Manager loaded");

        Rankings.Clear();
        globalLeaderboard = await SteamUserStats.FindOrCreateLeaderboardAsync("global_leaderboard", LeaderboardSort.Descending, LeaderboardDisplay.Numeric);

        if (globalLeaderboard.HasValue)
        {
            var results = await globalLeaderboard.Value.GetScoresAroundUserAsync(10, 10);

            if (results != null)
            {
                foreach (var ranking in results)
                {
                    Rankings.Add(new Ranking(ranking));
                }

                for (int i = 1; i <= 200; i++)
                {
                    if (!Rankings.Exists(x => x.Rank == i))
                    {
                        Rankings.Add(new Ranking(i));
                    }
                }

                Rankings = Rankings.OrderByDescending(x => x.Score).ToList();
                for (int i = 0; i < Rankings.Count; i++)
                {
                    Rankings[i].Rank = i + 1;
                }

            }
            else
            {
                Debug.LogError("Failed to get rankings");
                return;
            }

            OnLeaderboardUpdate.Invoke(Rankings);
        }
        else
        {
            Debug.LogError("Failed to get leaderboard");
            return;
        }

        Debug.Log("Success");
    }
}
