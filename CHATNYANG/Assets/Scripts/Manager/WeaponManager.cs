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
        Debug.Log($"[WeaponManager] Start / Registered Prefabs: {startingWeaponPrefabs.Count}");
        foreach (var prefab in startingWeaponPrefabs)
        {
            // 시작 무기는 기본적으로 Normal 등급으로 추가
            AddWeapon(prefab, WeaponRarity.Normal);
        }
    }

    // AI가 반환한 등급을 함께 받도록 파라미터 추가
    public bool AddWeapon(GameObject weaponPrefab, WeaponRarity drawnRarity = WeaponRarity.Normal)
    {
        WeaponBase existingWeapon = GetWeaponByPrefab(weaponPrefab);

        if (existingWeapon != null)
        {
            // 이미 들고 있는 무기라면 강화, 승급, 교체 로직으로 진입
            return UpgradeOrReplaceWeapon(existingWeapon, drawnRarity);
        }
        else
        {
            // 없는 무기라면 슬롯 확인 후 신규 장착
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

            weapon.InitializeWeapon(drawnRarity, 0);
            _weapons.Add(weapon);
            Debug.Log($"[WeaponManager] Added new weapon: {weaponPrefab.name} ({_weapons.Count}/{maxWeaponSlots})");

            // 무기 획득 또는 강화 성공 시 UI 새로고침 호출
            WeaponDisplayManager displayManager = FindObjectOfType<WeaponDisplayManager>();
            if (displayManager != null)
            {
                displayManager.RefreshUI();
            }

            return true;
        }
    }

    // 4가지 챗냥 기획 룰 판별 로직
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
                    // 룰 3: 3강에서 같은 게 나오면 다음 상위 등급 0강으로 승급
                    WeaponRarity nextRarity = (WeaponRarity)(currentRarityValue + 1);
                    existingWeapon.InitializeWeapon(nextRarity, 0);
                    Debug.Log($"[WeaponManager] Promoted to next rarity: {nextRarity} 0");
                    return true;
                }
            }
        }
        // 기획 외 예외: 현재 들고 있는 것보다 낮은 등급이 나오면 꽝 처리
        else
        {
            Debug.Log("[WeaponManager] Lower rarity drawn. Ignored.");
            return false;
        }
    }

    // 프리팹으로 이미 생성된 무기를 찾는 헬퍼 함수
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