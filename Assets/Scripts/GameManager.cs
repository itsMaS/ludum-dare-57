using MarTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using Steamworks;
using Steamworks.Data;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    public interface IGeneratable
    {
        public void Generate(params object[] argumants);
    }

    public class LeaderboardData
    {
        public LeaderboardEntry[] entries;

        public LeaderboardData(LeaderboardEntry[] entries)
        {
            this.entries = entries;
        }
    }

    [DefaultExecutionOrder(-200)]
    public class GameManager : Singleton<GameManager>, IMarkitEventCaller
    {
        public MarKitEvent OnRecordBeaten;
        public MarKitEvent OnGameOver;
        public UnityEvent<LeaderboardData> OnRankingsLoaded;

        public UnityEvent<int> OnScoreChanged;

        public override bool AddToDontDestroyOnLoad => false;

        public static int highscore
        {
            get
            {
                return PlayerPrefs.GetInt("highscore", 0);
            }
            set
            {
                PlayerPrefs.SetInt("highscore", value);
            }
        }

        [SerializeField] Transform killTrigger;
        [SerializeField] Transform recordMarker;
        [SerializeField] GameObject scoreIndicator;

        [SerializeField] float zoneMovementSpeed = 5;

        public List<LevelTemplate> Templates = new List<LevelTemplate>();
        public List<LevelTemplate> UsedTemplates = new List<LevelTemplate>();

        public PlayerController player { get; private set; }
        public float playerDepth => player.transform.position.y;

        private float nextHeight = 0;
        private float xPosition = 0;

        private bool gameStarted = false;
        bool scoreBeaten = false;
        private bool gameOver = false;

        Leaderboard? leaderboard;

        public int currentScore { get; private set; }

        public int comboLevel = 1;
        public float comboProgress = 0;

        protected override void Initialize()
        {
            base.Initialize();

            player = FindObjectOfType<PlayerController>();

            foreach (var item in FindObjectsOfType<Transform>())
            {
                if (item.TryGetComponent<IGeneratable>(out var a))
                {
                    a.Generate();
                }
            }

            comboLevel = 1;

            LoadLeaderboards();
        }

        private async void LoadLeaderboards()
        {
            leaderboard = await SteamUserStats.FindOrCreateLeaderboardAsync("GlobalHighscores", LeaderboardSort.Descending, LeaderboardDisplay.Numeric);


            if(leaderboard.HasValue)
            {
                var result = await leaderboard.Value.GetScoresAroundUserAsync(0, 0);

                if(result != null)
                {
                    highscore = result[0].Score;
                }
            } 
        }


        public void StartGame()
        {
            if (gameStarted) return;
            gameStarted = true;
        }

        public void AddScore(int score, Vector3 position)
        {
            int finalScore = comboLevel * score;
            currentScore += finalScore;

            OnScoreChanged.Invoke(currentScore);

            var scoreIndication = Instantiate(scoreIndicator, position, Quaternion.identity);
            TextMeshPro text = scoreIndication.GetComponentInChildren<TextMeshPro>();


            scoreIndication.transform.localScale = Vector3.one * ((float)finalScore).Remap(10, 400, 1, 5);

            scoreIndication.gameObject.SetActive(true);
            text.SetText($"+{finalScore}");
            Destroy(scoreIndication, 2);


            comboProgress += score / 60f;
            float leftOver = comboProgress % 1;

            if(comboProgress >= 1)
            {
                comboLevel++;
                comboProgress = leftOver;
            }
        }

        internal static async void GameOver()
        {
            if (Instance.gameOver) return;
            Instance.gameOver = true;

            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            int newScore = Instance.currentScore;

            if(Instance.currentScore > highscore)
            {
                highscore = Instance.currentScore;
                Instance.UpdateRecordPosition();
            }

            if(Instance.leaderboard.HasValue)
            {
                Debug.Log($"Name {SteamClient.Name}");

                Debug.Log($"Submitting {newScore} score");
                var result = await Instance.leaderboard.Value.SubmitScoreAsync(newScore);

                Debug.Log($"Submitted successfully? : {result.HasValue} Value: {result.Value.Score}");
                if(result.HasValue)
                {
                    Debug.Log($"New highscore? {result.Value.Changed} Old highscore {result.Value.OldGlobalRank} New highscore {result.Value.NewGlobalRank}");
                }
            }
            else
            {
                Debug.Log("Failed to submit to steam");
            }

            Instance.OnGameOver.Invoke(Instance);

            if(Instance.leaderboard.HasValue)
            {
                var result = await Instance.leaderboard.Value.GetScoresAroundUserAsync();

                Instance.OnRankingsLoaded.Invoke(new LeaderboardData(result));
            }

        }

        private void UpdateRecordPosition()
        {
            recordMarker.transform.position = new Vector3(0, -highscore, 0);
        }

        private void Start()
        {
            UpdateRecordPosition();
        }

        private void Update()
        {
            if(player.transform.position.y <= 20)
            {
                StartGame();
            }


            if(gameStarted)
            {
                killTrigger.transform.Translate(Vector2.down * Time.deltaTime * zoneMovementSpeed);
            }

            comboProgress -= Time.deltaTime * 0.025f * comboLevel;
            if(comboProgress <= 0)
            {
                if(comboLevel <= 1)
                {
                    comboProgress = 0;
                }
                else
                {
                    comboLevel--;
                    comboProgress = 1;
                }

            }



            if(playerDepth < nextHeight+100)
            {
                AddTemplate();
            }

            if(!gameOver && !scoreBeaten && currentScore > highscore)
            {
                scoreBeaten = true;
                OnRecordBeaten.Invoke(this);
            }

            if(Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void AddTemplate()
        {
            LevelTemplate selected = null;

#if UNITY_EDITOR
            if(LevelEditorWindow.debugLevel)
            {
                selected = LevelEditorWindow.debugLevel;
                LevelEditorWindow.debugLevel = null;
            }
            else
            {
#endif
                var notUsedTemplates = Templates.Where(x => !UsedTemplates.Contains(x));

                if(notUsedTemplates.Count() == 0)
                {
                    notUsedTemplates = Templates;
                    UsedTemplates.Clear();
                }
                selected = notUsedTemplates.PickRandom();
#if UNITY_EDITOR
            }
#endif
            UsedTemplates.Add(selected);

            var instance = Instantiate(selected, xPosition * Vector3.right + Vector3.down * (-nextHeight + selected.height/2), Quaternion.identity);
            nextHeight -= selected.height;

            foreach (var item in instance.GetComponentsInChildren<IGeneratable>())
            {
                item.Generate();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(Vector3.left*500 - Vector3.down * nextHeight, Vector3.right*500 - Vector3.down * nextHeight);
        }

        internal void ResetCombo()
        {
            comboLevel = 1;
            comboProgress = 0;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : MarToolsEditor<GameManager>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Reset score"))
            {
                GameManager.highscore = 0;
            }
        }
    }
#endif
}
