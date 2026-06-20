using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Starting Weapons")]
    [SerializeField] private List<GameObject> startingWeaponPrefabs;

    [Header("Settings")]
    [SerializeField] private int maxWeaponSlots = 6;

    private List<WeaponBase> _weapons = new();

    public IReadOnlyList<WeaponBase> Weapons => _weapons;

    private void Start()
    {
        // 타이틀에서 선택한 무기가 있으면 그걸로 시작하되, 프리팹에 설정된 등급/레벨을 그대로 가져옵니다.
        if (WeaponSelectData.Instance != null && WeaponSelectData.Instance.SelectedWeaponPrefab != null)
        {
            WeaponBase baseStats = WeaponSelectData.Instance.SelectedWeaponPrefab.GetComponent<WeaponBase>();
            AddWeapon(WeaponSelectData.Instance.SelectedWeaponPrefab, baseStats.currentRarity, baseStats.currentUpgradeLevel);
        }
        else
        {
            // 선택 없을 시 기본 무기로 폴백할 때도 프리팹 인스펙터의 설정을 존중합니다.
            foreach (var prefab in startingWeaponPrefabs)
            {
                WeaponBase baseStats = prefab.GetComponent<WeaponBase>();
                AddWeapon(prefab, baseStats.currentRarity, baseStats.currentUpgradeLevel);
            }
        }
    }

    // AI의 답변에 맞춰 무기를 추가할 때 호출되는 함수 (기본값은 노말 0강으로 유지하되, 지정 가능하도록 수정)
    public bool AddWeapon(GameObject weaponPrefab, WeaponRarity drawnRarity = WeaponRarity.Normal, int drawnUpgradeLevel = 0)
    {
        WeaponBase existingWeapon = GetWeaponByPrefab(weaponPrefab);

        if (existingWeapon != null)
        {
            // 이미 갖고 있는 무기면 진화, 돌파, 교체 알고리즘 실행
            return UpgradeOrReplaceWeapon(existingWeapon, drawnRarity);
        }
        else
        {
            // 빈 슬롯이 없으면 무기 획득 실패 처리
            if (_weapons.Count >= maxWeaponSlots)
            {
                Debug.LogWarning("[WeaponManager] Weapon slots are full.");
                return false;
            }

            GameObject go = Instantiate(weaponPrefab, transform);
            WeaponBase weapon = go.GetComponent<WeaponBase>();

            if (weapon == null)
            {
                Debug.LogError($"[WeaponManager] WeaponBase component missing: {weaponPrefab.name}");
                Destroy(go);
                return false;
            }

            // 인스펙터에서 가져온 값이나 뽑기에서 나온 값으로 초기화 (기존 0 고정에서 수정됨)
            weapon.InitializeWeapon(drawnRarity, drawnUpgradeLevel);
            _weapons.Add(weapon);
            Debug.Log($"[WeaponManager] Added new weapon: {weaponPrefab.name} ({_weapons.Count}/{maxWeaponSlots})");

            // 무기 획득 또는 진화 성공 시 UI 새로고침 호출
            WeaponDisplayManager displayManager = FindObjectOfType<WeaponDisplayManager>();
            if (displayManager != null)
            {
                displayManager.RefreshUI();
            }

            return true;
        }
    }

    // 4가지 등급 진화 및 돌파 로직
    private bool UpgradeOrReplaceWeapon(WeaponBase existingWeapon, WeaponRarity drawnRarity)
    {
        int currentRarityValue = (int)existingWeapon.currentRarity;
        int drawnRarityValue = (int)drawnRarity;

        // 룰 1: 더 높은 등급이 나온 경우 (교체)
        if (drawnRarityValue > currentRarityValue)
        {
            existingWeapon.InitializeWeapon(drawnRarity, 0);
            Debug.Log($"[WeaponManager] Replaced with higher rarity: {drawnRarity} 0");
            return true;
        }
        // 룰 2 & 3 & 4: 같은 등급이 나온 경우
        else if (drawnRarityValue == currentRarityValue)
        {
            if (existingWeapon.currentUpgradeLevel < 3)
            {
                // 룰 2: 3강 미만이면 강화
                existingWeapon.InitializeWeapon(existingWeapon.currentRarity, existingWeapon.currentUpgradeLevel + 1);
                Debug.Log($"[WeaponManager] Upgraded: {existingWeapon.currentRarity} {existingWeapon.currentUpgradeLevel}");
                return true;
            }
            else
            {
                // 룰 4: 최고 등급(Legendary) 3강에서 또 같은 게 나오면 꽝 처리
                if (existingWeapon.currentRarity == WeaponRarity.Legendary)
                {
                    Debug.Log("[WeaponManager] Legendary Max Level duplicate. Considered as trash.");
                    return false;
                }
                else
                {
                    // 룰 3: 3강에서 같은 게 나오면 다음 등급 0강으로 돌파
                    WeaponRarity nextRarity = (WeaponRarity)(currentRarityValue + 1);
                    existingWeapon.InitializeWeapon(nextRarity, 0);
                    Debug.Log($"[WeaponManager] Promoted to next rarity: {nextRarity} 0");
                    return true;
                }
            }
        }
        // 뽑기 꽝 판정: 현재 들고 있는 것보다 낮은 등급이 나오면 꽝 처리
        else
        {
            Debug.Log("[WeaponManager] Lower rarity drawn. Ignored.");
            return false;
        }
    }

    // 프리팹으로 이미 장착된 무기를 찾는 유틸 함수
    private WeaponBase GetWeaponByPrefab(GameObject prefab)
    {
        string prefabName = prefab.name + "(Clone)";
        return _weapons.Find(w => w.gameObject.name == prefabName);
    }

    public bool HasWeapon(GameObject prefab)
    {
        return GetWeaponByPrefab(prefab) != null;
    }

    public bool HasWeaponByData(WeaponData data)
    {
        return _weapons.Exists(w => w.WeaponData == data);
    }

    public int WeaponCount => _weapons.Count;
    public bool IsFull => _weapons.Count >= maxWeaponSlots;
}