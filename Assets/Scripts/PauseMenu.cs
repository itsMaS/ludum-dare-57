using MarKit;
using MarTools;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenu : Singleton<PauseMenu>
{
    [SerializeField] UnityEngine.UI.Button restartButton;
    [SerializeField] UnityEngine.UI.Button playButton;
    [SerializeField] UnityEngine.UI.Button resumeButton;

    [SerializeField] GroupBehavior gameOverSection;
    [SerializeField] GroupBehavior pauseMenu;
    [SerializeField] AudioMixer audioMixer;

    CanvasGroup cg;

    public UnityEvent OnClose;
    public UnityEvent OnOpen;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();
        GameManager.Instance.Pause();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Resume()
    {
        ClickButton(() =>
        {
            GameManager.Instance.Unpause();
        });
    }

    public void Restart()
    {
        ClickButton(() =>
        {
            GameManager.Instance.Restart();
        });
    }

    internal static void Close()
    {
        Instance.DelayedAction(0.5f, null, t => Instance.audioMixer.SetFloat("Low_Pass", t.Remap01(800, 22000)), false);
        Instance.OnClose.Invoke();
        Instance.ClickButton(() => Instance.gameObject.SetActive(false), 0.5f);
    }

    internal static void Open()
    {
        Instance.gameObject.SetActive(true);
        Instance.DelayedAction(0.5f, null, t => Instance.audioMixer.SetFloat("Low_Pass", t.Remap01(22000, 800)), false);

        if(GameManager.Instance.gameStarted && !GameManager.Instance.gameOver)
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

        Instance.pauseMenu.Activate();
        Instance.OnOpen.Invoke();
    }

    public static void OpenEndWindow()
    {
        Open();

        Instance.gameOverSection.Activate();
    }

    public void ClickButton(UnityAction action, float duration = 0.1f)
    {
        cg.interactable = false;
        cg.blocksRaycasts = false;

        this.DelayedAction(duration, () =>
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            action?.Invoke();
        }, null, false);
    }

    public void Navigate(GroupBehavior groupBehavior)
    {
        ClickButton(() => groupBehavior.Activate());
    }
}
