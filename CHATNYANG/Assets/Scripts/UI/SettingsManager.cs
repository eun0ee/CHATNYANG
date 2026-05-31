using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;

    // 슬라이더 값 연동을 위해 선언해 둠 (나중에 오디오 매니저 등과 연결)
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool _isSettingsOpen = false;

    private void Start()
    {
        // 시작 시 패널이 열려있지 않도록 끔
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 닫기 버튼 이벤트 등록
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
    }

    private void Update()
    {
        // ESC 키 입력 감지
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
        // Time.timeScale = _isSettingsOpen ? 0f : 1f;
    }

    public void CloseSettings()
    {
        _isSettingsOpen = false;
        settingsPanel.SetActive(false);

        // 일시정지를 사용했다면 다시 시간을 흐르게 함
        // Time.timeScale = 1f;
    }
}