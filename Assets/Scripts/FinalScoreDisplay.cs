using MarKit;
using MarTools;
using TMPro;
using UnityEngine;

public class FinalScoreDisplay : MarKitBehavior
{
    public MarKitEvent OnHighscoreReached;
    public MarKitEvent OnShowed;

    [SerializeField] MultiTextMeshPro multiText;
    [SerializeField] TextMeshProUGUI previousHighscoreText;

    private void OnEnable()
    {
        OnShowed.Invoke(this);

        int from = 0;
        int currentScore = GameManager.Instance.currentScore;
        int previousHighscore = GameManager.Instance.previousHighscore;

        previousHighscoreText.SetText(previousHighscore.ToString());

        this.DelayedAction(2, () =>
        {
            if(currentScore > previousHighscore)
            {
                OnHighscoreReached.Invoke(this);
            }
        }, t =>
        {
            int count = Mathf.CeilToInt(Mathf.Lerp((float)from, (float)currentScore, t));
            multiText.SetText(count.ToString());
        }, false, Utilities.Ease.OutQuad);
    }
}
