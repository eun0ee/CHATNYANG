using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject dimBackground;
    [SerializeField] private Button closeButton;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool _isSettingsOpen = false;

    private void Start()
    {
        // 시작 시 패널과 어두운 배경이 열려있지 않도록 비활성화
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (dimBackground != null)
        {
            dimBackground.SetActive(false);
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

        // 설정창 상태에 맞춰 어두운 배경도 켜고 끔
        if (dimBackground != null)
        {
            dimBackground.SetActive(_isSettingsOpen);
        }

        // 설정창이 열렸을 때 게임을 일시정지
        Time.timeScale = _isSettingsOpen ? 0f : 1f;
    }

    public void CloseSettings()
    {
        _isSettingsOpen = false;
        settingsPanel.SetActive(false);

        // 닫을 때 어두운 배경도 끔
        if (dimBackground != null)
        {
            dimBackground.SetActive(false);
        }

        // 일시정지를 해제하고 다시 시간을 흐르게 함
        Time.timeScale = 1f;
    }

    private void SetBGMVolume(float volume)
    {
        // volume 값이 무조건 0.0001과 1 사이에서만 놀도록 강제 고정
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);

        if (mainMixer != null)
        {
            mainMixer.SetFloat("BGMVolume", Mathf.Log10(safeVolume) * 20f);
        }
    }

    private void SetSFXVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);

        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(safeVolume) * 20f);
        }
    }
}