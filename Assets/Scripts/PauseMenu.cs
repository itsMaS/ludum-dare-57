using MarKit;
using MarTools;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenu : Singleton<PauseMenu>
{
    [SerializeField] UnityEngine.UI.Button restartButton;
    [SerializeField] UnityEngine.UI.Button playButton;
    [SerializeField] UnityEngine.UI.Button resumeButton;
    [SerializeField] UnityEngine.UI.Button quitButton;

    CanvasGroup cg;

    public UnityEvent OnClose;
    public UnityEvent OnOpen;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();

        restartButton.onClick.AddListener(() => ClickButton(Restart));
        playButton.onClick.AddListener(Play);
        resumeButton.onClick.AddListener(Resume);
        quitButton.onClick.AddListener(() => ClickButton(Quit));

        GameManager.Instance.Pause();
    }

    private void Quit()
    {
        Application.Quit();
    }

    private void Play()
    {
        GameManager.Instance.Restart();
    }

    private void Resume()
    {
        GameManager.Instance.Unpause();
    }

    private void Restart()
    {
        GameManager.Instance.Restart();
    }

    internal static void Close()
    {
        Instance.OnClose.Invoke();
        Instance.ClickButton(() => Instance.gameObject.SetActive(false));
    }

    internal static void Open()
    {
        Instance.gameObject.SetActive(true);

        if(GameManager.Instance.gameStarted)
        {
            Instance.restartButton.gameObject.SetActive(true);
            Instance.resumeButton.gameObject.SetActive(true);
            Instance.playButton.gameObject.SetActive(false);
        }
        else 
        {
            Instance.restartButton.gameObject.SetActive(false);
            Instance.resumeButton.gameObject.SetActive(false);
            Instance.playButton.gameObject.SetActive(true);
        }

        Instance.OnOpen.Invoke();
    }

    public void ClickButton(UnityAction action)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        this.DelayedAction(0.2f, () =>
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            action?.Invoke();
        }, null, false);
    }
}
