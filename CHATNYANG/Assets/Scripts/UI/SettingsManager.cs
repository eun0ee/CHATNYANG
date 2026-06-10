using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private PanelAnimator settingsPanelAnimator;
    [SerializeField] private PanelAnimator dimBackgroundAnimator; // 있는 경우
    [SerializeField] private Button closeButton;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
{
    // 연결 상태 확인용 로그 추가
    Debug.Log($"settingsPanelAnimator: {settingsPanelAnimator}");
    
    closeButton?.onClick.AddListener(CloseSettings);
    bgmSlider?.onValueChanged.AddListener(SetBGMVolume);
    sfxSlider?.onValueChanged.AddListener(SetSFXVolume);
}

private void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape) && settingsPanelAnimator != null)
        ToggleSettings();
}

    private void ToggleSettings()
    {
        if (settingsPanelAnimator.IsVisible)
            CloseSettings();
        else
            OpenSettings();
    }

    public void OpenSettings()
    {
        Time.timeScale = 0f;
        settingsPanelAnimator.Show();
        dimBackgroundAnimator?.Show();
    }

    public void CloseSettings()
    {
        settingsPanelAnimator.Hide(() =>
        {
            Time.timeScale = 1f;
        });
        dimBackgroundAnimator?.Hide();
    }

    private void SetBGMVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        mainMixer?.SetFloat("BGMVolume", Mathf.Log10(safeVolume) * 20f);
    }

    private void SetSFXVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        mainMixer?.SetFloat("SFXVolume", Mathf.Log10(safeVolume) * 20f);
    }
}