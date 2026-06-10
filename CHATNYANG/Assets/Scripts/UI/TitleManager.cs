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

    [Header("Weapon Select UI")]
    [SerializeField] private GameObject weaponSelectPanel; // 무기 선택 패널
    [SerializeField] private WeaponSelectUI weaponSelectUI; // 아래에서 만들 컴포넌트

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BgmType.Title);

        settingsPanel?.SetActive(false);
        weaponSelectPanel?.SetActive(false);

        startButton?.onClick.AddListener(OpenWeaponSelect); // 바로 로드 대신 패널 열기
        settingButton?.onClick.AddListener(OpenSettings);
    }

    private void OpenWeaponSelect()
    {
        weaponSelectPanel?.SetActive(true);
    }

    private void OpenSettings()
    {
        settingsPanel?.SetActive(true);
    }

    public void OnWeaponConfirmed()
    {
        weaponSelectPanel.SetActive(false);
        SceneManager.LoadScene("GamePlay");
    }
}