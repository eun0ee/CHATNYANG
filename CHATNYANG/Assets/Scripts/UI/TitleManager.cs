using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;

    [Header("Panels")]
    [SerializeField] private PanelAnimator settingsPanelAnimator;
    [SerializeField] private PanelAnimator weaponSelectPanelAnimator;

    [Header("Weapon Select UI")]
    [SerializeField] private WeaponSelectUI weaponSelectUI;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BgmType.Title);

        startButton?.onClick.AddListener(OpenWeaponSelect);
        settingButton?.onClick.AddListener(OpenSettings);
    }

    private void OpenWeaponSelect()
    {
        weaponSelectPanelAnimator.Show();
    }

    private void OpenSettings()
    {
        settingsPanelAnimator.Show();
    }

    // 설정창 닫기 버튼에서 호출
    public void CloseSettings()
    {
        settingsPanelAnimator.Hide();
    }

    public void OnWeaponConfirmed()
    {
        weaponSelectPanelAnimator.Hide(() =>
        {
            SceneManager.LoadScene("GamePlay");
        });
    }
    public void CloseWeaponSelect()
    {
        weaponSelectPanelAnimator.Hide();
    }
}