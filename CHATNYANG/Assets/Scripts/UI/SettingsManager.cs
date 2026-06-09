using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool _isSettingsOpen = false;

    private void Start()
    {
        // 시작 시 패널이 열려있지 않도록 비활성화
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 닫기 버튼 이벤트 등록
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        // 슬라이더 값이 변경될 때마다 볼륨 조절 함수가 호출되도록 리스너 등록
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private void Update()
    {
        // ESC 키 입력 감지하여 설정창 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    private void ToggleSettings()
    {
        _isSettingsOpen = !_isSettingsOpen;
        settingsPanel.SetActive(_isSettingsOpen);

        // 설정창이 열렸을 때 게임을 일시정지하고 싶다면 아래 주석 해제
        Time.timeScale = _isSettingsOpen ? 0f : 1f;
    }

    public void CloseSettings()
    {
        _isSettingsOpen = false;
        settingsPanel.SetActive(false);

        // 일시정지를 사용했다면 다시 시간을 흐르게 함
        Time.timeScale = 1f;
    }

    private void SetBGMVolume(float volume)
    {
        // 슬라이더 값(0.0001 ~ 1)을 데시벨(-80dB ~ 0dB)로 변환하여 믹서에 적용
        if (mainMixer != null)
        {
            mainMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20f);
        }
    }

    private void SetSFXVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
        }
    }
}