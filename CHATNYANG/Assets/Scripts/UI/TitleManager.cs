using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;

    [Header("Settings UI")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BgmType.Title);

        // 환경설정 창은 처음에 꺼둠
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 버튼 클릭 이벤트 연결
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadMainGame);
        }

        if (settingButton != null)
        {
            settingButton.onClick.AddListener(OpenSettings);
        }
    }

    private void LoadMainGame()
    {
        // 씬 이름이나 인덱스를 입력하여 메인 게임으로 이동
        // Build Settings에 메인 씬이 등록되어 있어야 함
        SceneManager.LoadScene("GamePlay");
    }

    private void OpenSettings()
    {
        // 설정창 켜기
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }
}