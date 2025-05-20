using MarTools;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;

    [SerializeField] SliderInputBehavior SFXVolumeSetting;
    [SerializeField] SliderInputBehavior MusicVolumeSetting;
    [SerializeField] SliderInputBehavior MasterVolumeSetting;

    [SerializeField] AudioSource sfxTest;

    private void Start()
    {
        InitializeAudioSetting(SFXVolumeSetting, "SFX_Volume", "SFX_Volume", () => sfxTest.Play());
        InitializeAudioSetting(MusicVolumeSetting, "Music_Volume", "Music_Volume");
        InitializeAudioSetting(MasterVolumeSetting, "Master_Volume", "Master_Volume");
    }

    private void InitializeAudioSetting(SliderInputBehavior slider, string mixerParameter, string saveKey, Action onChanged = null)
    {
        float savedParameter = PlayerPrefs.GetFloat(saveKey, 0.5f);
        SetVolume(savedParameter, mixerParameter);
        slider.SetValue(savedParameter);
        
        slider.OnUpdateValue.AddListener(t =>
        {
            SetVolume(t, mixerParameter);
            PlayerPrefs.SetFloat(saveKey, t);
            onChanged?.Invoke();
        });
    }
    private void SetVolume(float progress, string mixerParameter)
    {
        audioMixer.SetFloat(mixerParameter, Volume01ToDb(progress));
    }

    float Volume01ToDb(float volume01)
    {
        return Mathf.Log10(Mathf.Clamp(volume01, 0.0001f, 1f)) * 20f;
    }

    public void ResetHighscore()
    {
        LeaderboardManager.Instance.ResetHighscore();
    }
}
