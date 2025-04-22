using MarTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    public interface IGeneratable
    {
        public void Generate(params object[] argumants);
    }

    public class GameManager : Singleton<GameManager>, IMarkitEventCaller
    {
        public MarKitEvent OnRecordBeaten;
        public MarKitEvent OnGameOver;

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

        public int currentScore => -Mathf.RoundToInt(playerDepth);

        [SerializeField] Transform killTrigger;
        [SerializeField] Transform recordMarker;

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
        }

        public void StartGame()
        {
            gameStarted = true;
        }

        internal static void GameOver()
        {
            if (Instance.gameOver) return;
            Instance.gameOver = true;

            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            Debug.Log(Instance.currentScore);

            if(Instance.currentScore > highscore)
            {
                highscore = Instance.currentScore;
                Instance.UpdateRecordPosition();
            }

            Instance.OnGameOver.Invoke(Instance);
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
            if(gameStarted)
            {
                killTrigger.transform.Translate(Vector2.down * Time.deltaTime * zoneMovementSpeed);
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
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(Vector3.left*500 - Vector3.down * nextHeight, Vector3.right*500 - Vector3.down * nextHeight);
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
