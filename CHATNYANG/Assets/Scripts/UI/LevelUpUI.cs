using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button[] optionButtons;       // 카드 3장
    [SerializeField] private Image[] optionIcons;
    [SerializeField] private TextMeshProUGUI[] optionNames;
    [SerializeField] private TextMeshProUGUI[] optionDescs;

    [Header("Upgrade Pool")]
    [SerializeField] private UpgradeOptionData[] allUpgrades; // 전체 강화 목록

    private ExperienceSystem _expSystem;
    private WeaponManager _weaponManager;
    private PlayerStats _playerStats;
    private List<UpgradeOptionData> _currentOptions = new();

    private void Start()
    {
        _expSystem = FindObjectOfType<ExperienceSystem>();
        _weaponManager = FindObjectOfType<WeaponManager>();
        _playerStats = FindObjectOfType<PlayerStats>();

        Debug.Log($"[LevelUpUI] ExpSystem: {_expSystem != null} / WeaponManager: {_weaponManager != null} / PlayerStats: {_playerStats != null}");

        if (_expSystem != null)
            _expSystem.OnLevelUp += ShowLevelUpPanel;
        else
            Debug.LogError("[LevelUpUI] ExperienceSystem을 찾지 못했습니다!");

        panelRoot.SetActive(false);
    }

    private void ShowLevelUpPanel()
    {
        Debug.Log($"[LevelUpUI] ShowLevelUpPanel 호출 / 옵션 수: {allUpgrades.Length}");

        Time.timeScale = 0f;
        panelRoot.SetActive(true);

        _currentOptions = PickRandomOptions(3);
        Debug.Log($"[LevelUpUI] 뽑힌 옵션 수: {_currentOptions.Count}");

        for (int i = 0; i < optionButtons.Length; i++)
        {
            // 뽑힌 옵션이 버튼 수보다 적을 수 있으니 체크
            if (i >= _currentOptions.Count)
            {
                optionButtons[i].gameObject.SetActive(false);
                continue;
            }

            optionButtons[i].gameObject.SetActive(true);

            var option = _currentOptions[i];

            // 아이콘 null 체크
            if (i < optionIcons.Length && optionIcons[i] != null)
                optionIcons[i].sprite = option.icon;

            // 이름 null 체크
            if (i < optionNames.Length && optionNames[i] != null)
                optionNames[i].text = option.upgradeName;

            // 설명 null 체크
            if (i < optionDescs.Length && optionDescs[i] != null)
                optionDescs[i].text = option.description;

            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => SelectUpgrade(index));
        }
    }

    private void SelectUpgrade(int index)
    {
        ApplyUpgrade(_currentOptions[index]);

        panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ApplyUpgrade(UpgradeOptionData option)
    {
        switch (option.upgradeType)
        {
            case UpgradeType.NewWeapon:
                _weaponManager?.AddWeapon(option.weaponPrefab);
                break;

            case UpgradeType.WeaponDamage:
                UpgradeMatchingWeapon(option);
                break;

            case UpgradeType.WeaponCooldown:
                UpgradeMatchingWeapon(option);
                break;

            case UpgradeType.WeaponCount:
                UpgradeMatchingWeapon(option);
                break;

            case UpgradeType.WeaponPierce:
                UpgradeMatchingWeapon(option);
                break;

            case UpgradeType.PlayerMaxHp:
                _playerStats.maxHp += option.floatValue;
                _playerStats.currentHp += option.floatValue;
                break;

            case UpgradeType.PlayerMoveSpeed:
                _playerStats.moveSpeed += option.floatValue;
                break;

            case UpgradeType.PlayerArmor:
                _playerStats.armor += option.floatValue;
                break;

            case UpgradeType.PlayerRecovery:
                _playerStats.recovery += option.floatValue;
                break;
        }
    }

    private void UpgradeMatchingWeapon(UpgradeOptionData option)
    {
        foreach (var weapon in _weaponManager.Weapons)
        {
            // WeaponBase에 WeaponData 프로퍼티 노출 필요 (아래 참고)
            if (weapon is WeaponBase wb && wb.WeaponData == option.targetWeapon)
            {
                switch (option.upgradeType)
                {
                    case UpgradeType.WeaponDamage:
                        option.targetWeapon.damage += option.floatValue;
                        break;
                    case UpgradeType.WeaponCooldown:
                        option.targetWeapon.attackCooldown *= option.floatValue;
                        break;
                    case UpgradeType.WeaponCount:
                        option.targetWeapon.projectileCount += option.intValue;
                        break;
                    case UpgradeType.WeaponPierce:
                        option.targetWeapon.bounceCount += option.intValue;
                        break;
                }
                break;
            }
        }
    }

    private List<UpgradeOptionData> PickRandomOptions(int count)
    {
        List<UpgradeOptionData> pool = new(allUpgrades);
        List<UpgradeOptionData> result = new();

        // 이미 보유한 무기의 NewWeapon 옵션 제거
        pool.RemoveAll(u =>
            u.upgradeType == UpgradeType.NewWeapon &&
            _weaponManager.HasWeapon(u.weaponPrefab)
        );

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int randIdx = Random.Range(0, pool.Count);
            result.Add(pool[randIdx]);
            pool.RemoveAt(randIdx);
        }

        return result;
    }
}