using MarTools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
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


    [DefaultExecutionOrder(-200)]
    public class GameManager : Singleton<GameManager>, IMarkitEventCaller
    {
        public MarKitEvent OnRecordBeaten;
        public MarKitEvent OnGameOver;
        public MarKitEvent OnRestart;

        public UnityEvent<int> OnScoreChanged;
        public UnityEvent OnComboGained;
        public UnityEvent OnComboLost;

        public override bool AddToDontDestroyOnLoad => true;

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
        [SerializeField] RectTransform cursorTr;

        [SerializeField] float zoneMovementSpeed = 5;

        public List<LevelTemplate> Templates = new List<LevelTemplate>();
        public List<LevelTemplate> UsedTemplates = new List<LevelTemplate>();

        public PlayerController player { get; private set; }
        public float playerDepth => player.transform.position.y;

        private float nextHeight = 0;
        private float xPosition = 0;

        public bool gameStarted { get; private set; } = false;
        bool scoreBeaten = false;
        private bool gameOver = false;

        public int currentScore { get; private set; }

        public int comboLevel = 1;
        public float comboProgress = 0;

        public List<LevelTemplate> LoadedLevels = new List<LevelTemplate>();

        private bool isPaused = false;

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
        }

        public void StartGame()
        {
            if (gameStarted) return;
            gameStarted = true;

            isPaused = true;
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
                OnComboGained.Invoke();
                comboProgress = leftOver;
            }
        }

        internal static void GameOver()
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

            Instance.gameStarted = false;
            Instance.OnGameOver.Invoke(Instance);

            Instance.Pause();
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
            Cursor.visible = false;

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
                    OnComboLost.Invoke();
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

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(isPaused)
                {
                    Unpause();
                }
                else
                {
                    Pause();
                }
            }

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursorTr.parent as RectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );
            cursorTr.anchoredPosition = localPoint;
        }

        public void Restart()
        {
            nextHeight = 0;

            gameOver = false;
            gameStarted = false;
            killTrigger.transform.position = new Vector3(0, 100, 0);

            player.Respawn();

            foreach (var item in LoadedLevels)
            {
                Destroy(item.gameObject);
            }

            foreach (var item in FindObjectsOfType<BulletBehavior>())
            {
                Destroy(item.gameObject);
            }

            LoadedLevels.Clear();
            OnRestart.Invoke(this);

            currentScore = 0;
            OnScoreChanged.Invoke(currentScore);

            if(isPaused)
            {
                Unpause();
            }
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

            LoadedLevels.Add(instance);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(Vector3.left*500 - Vector3.down * nextHeight, Vector3.right*500 - Vector3.down * nextHeight);
        }

        internal void ResetCombo()
        {
            comboLevel = 1;
            comboProgress = 0;

            OnComboLost.Invoke();
        }

        internal void Unpause()
        {
            isPaused = false;
            PauseMenu.Close();

            Time.timeScale = 1;
        }
        public void Pause()
        {
            isPaused = true;
            PauseMenu.Open();

            Time.timeScale = 0;
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
