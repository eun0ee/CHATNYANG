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
        // 타이틀에서 선택한 무기가 있으면 그걸로 시작
        if (WeaponSelectData.Instance != null && WeaponSelectData.Instance.SelectedWeaponPrefab != null)
        {
            AddWeapon(WeaponSelectData.Instance.SelectedWeaponPrefab, WeaponRarity.Normal);
        }
        else
        {
            // 선택 없을 시 기본 무기로 폴백
            foreach (var prefab in startingWeaponPrefabs)
                AddWeapon(prefab, WeaponRarity.Normal);
        }
    }

    // AI�� ��ȯ�� ����� �Բ� �޵��� �Ķ���� �߰�
    public bool AddWeapon(GameObject weaponPrefab, WeaponRarity drawnRarity = WeaponRarity.Normal)
    {
        WeaponBase existingWeapon = GetWeaponByPrefab(weaponPrefab);

        if (existingWeapon != null)
        {
            // �̹� ��� �ִ� ������ ��ȭ, �±�, ��ü �������� ����
            return UpgradeOrReplaceWeapon(existingWeapon, drawnRarity);
        }
        else
        {
            // ���� ������ ���� Ȯ�� �� �ű� ����
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

            // ���� ȹ�� �Ǵ� ��ȭ ���� �� UI ���ΰ�ħ ȣ��
            WeaponDisplayManager displayManager = FindObjectOfType<WeaponDisplayManager>();
            if (displayManager != null)
            {
                displayManager.RefreshUI();
            }

            return true;
        }
    }

    // 4���� ê�� ��ȹ �� �Ǻ� ����
    private bool UpgradeOrReplaceWeapon(WeaponBase existingWeapon, WeaponRarity drawnRarity)
    {
        int currentRarityValue = (int)existingWeapon.currentRarity;
        int drawnRarityValue = (int)drawnRarity;

        // �� 1: �� ���� ����� ���� ��� (��ü)
        if (drawnRarityValue > currentRarityValue)
        {
            existingWeapon.InitializeWeapon(drawnRarity, 0);
            Debug.Log($"[WeaponManager] Replaced with higher rarity: {drawnRarity} 0");
            return true;
        }
        // �� 2 & 3 & 4: ���� ����� ���� ���
        else if (drawnRarityValue == currentRarityValue)
        {
            if (existingWeapon.currentUpgradeLevel < 3)
            {
                // �� 2: 3�� �̸��̸� ��ȭ
                existingWeapon.InitializeWeapon(existingWeapon.currentRarity, existingWeapon.currentUpgradeLevel + 1);
                Debug.Log($"[WeaponManager] Upgraded: {existingWeapon.currentRarity} {existingWeapon.currentUpgradeLevel}");
                return true;
            }
            else
            {
                // �� 4: �ְ� ���(Legendary) 3������ �� ���� �� ������ �� ó��
                if (existingWeapon.currentRarity == WeaponRarity.Legendary)
                {
                    Debug.Log("[WeaponManager] Legendary Max Level duplicate. Considered as trash.");
                    return false;
                }
                else
                {
                    // �� 3: 3������ ���� �� ������ ���� ���� ��� 0������ �±�
                    WeaponRarity nextRarity = (WeaponRarity)(currentRarityValue + 1);
                    existingWeapon.InitializeWeapon(nextRarity, 0);
                    Debug.Log($"[WeaponManager] Promoted to next rarity: {nextRarity} 0");
                    return true;
                }
            }
        }
        // ��ȹ �� ����: ���� ��� �ִ� �ͺ��� ���� ����� ������ �� ó��
        else
        {
            Debug.Log("[WeaponManager] Lower rarity drawn. Ignored.");
            return false;
        }
    }

    // ���������� �̹� ������ ���⸦ ã�� ���� �Լ�
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