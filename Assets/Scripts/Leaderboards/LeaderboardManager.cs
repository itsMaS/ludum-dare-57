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
            Rank = entry.GlobalRank;

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

    [System.Serializable]
    public class LeaderboardResult
    {
        public Ranking playerRank => Rankings.Find(x => x.isYou);
        public List<Ranking> Rankings = new List<Ranking>();
    }

    Leaderboard? globalLeaderboard;

    LeaderboardResult result = null;
    System.Action<LeaderboardResult> leaderboardAsk = null;

    public UnityEvent<LeaderboardResult> OnFetched;
    bool isLoading = false;


    private void Start()
    {
        GameManager.Instance.OnGameOver.AddListener(GameOver);
        FetchLeaderboards();
    }

    private async void GameOver()
    {
        if(globalLeaderboard.HasValue)
        {
            result = null;
            isLoading = true;
            await globalLeaderboard.Value.SubmitScoreAsync(GameManager.Instance.currentScore);
            isLoading = false;
            
            FetchLeaderboards();
        }
    }

    public async void FetchLeaderboards()
    {
        Debug.Log("Trying to fetch leaderboards");
        if (isLoading) return;

        if(!Steamworks.SteamClient.IsValid)
        {
            SendNegativeAnswer();
            Debug.Log("Steam not initialized");
            return;
        }

        Debug.Log("Fetching leaderboards");
        isLoading = true;

        result = null;
        globalLeaderboard = await SteamUserStats.FindOrCreateLeaderboardAsync("global_leaderboard", LeaderboardSort.Descending, LeaderboardDisplay.Numeric);

        if (globalLeaderboard.HasValue)
        {
            var results = await globalLeaderboard.Value.GetScoresAroundUserAsync(2, 2);

            result = new LeaderboardResult();
            if (results != null)
            {
                foreach (var ranking in results)
                {
                    var r = new Ranking(ranking);
                    result.Rankings.Add(r);
                }

                leaderboardAsk?.Invoke(result);
                leaderboardAsk = null;

                OnFetched.Invoke(result);
                Debug.Log("Success");
            }
            else
            {
                Debug.LogError($"Failed to get rankings");
                SendNegativeAnswer();
                return;
            }
        }
        else
        {
            SendNegativeAnswer();
            Debug.LogError("Failed to get leaderboard");
            return;
        }

        void SendNegativeAnswer()
        {
            leaderboardAsk?.Invoke(null);
            leaderboardAsk = null;
            isLoading = false;
        }
    }

    internal void RequestLeaderboard(System.Action<LeaderboardResult> leaderboardsLoaded)
    {
        if(result != null)
        {
            leaderboardsLoaded.Invoke(result);
        }
        else
        {
            leaderboardAsk = leaderboardsLoaded;
            FetchLeaderboards();
        }
    }

    internal void ResetHighscore()
    {
        if(globalLeaderboard.HasValue)
        {
            globalLeaderboard.Value.ReplaceScore(0);
            result = null;
        }
        GameManager.highscore = 0;
    }
}
