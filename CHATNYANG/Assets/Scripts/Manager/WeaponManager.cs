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
        Debug.Log($"[WeaponManager] Start / 등록된 프리팹 수: {startingWeaponPrefabs.Count}");
        foreach (var prefab in startingWeaponPrefabs)
        {
            Debug.Log($"[WeaponManager] 무기 추가 시도: {prefab?.name ?? "NULL"}");
            AddWeapon(prefab);
        }
    }

    public bool AddWeapon(GameObject weaponPrefab)
    {
        if (_weapons.Count >= maxWeaponSlots)
        {
            Debug.LogWarning("[WeaponManager] 무기 슬롯이 가득 찼습니다.");
            return false;
        }

        if (HasWeapon(weaponPrefab))
        {
            Debug.LogWarning($"[WeaponManager] 이미 보유한 무기입니다: {weaponPrefab.name}");
            return false;
        }

        // 플레이어 자식으로 생성 위치 자동 따라옴
        GameObject go = Instantiate(weaponPrefab, transform);
        WeaponBase weapon = go.GetComponent<WeaponBase>();

        if (weapon == null)
        {
            Debug.LogError($"[WeaponManager] WeaponBase 컴포넌트가 없습니다: {weaponPrefab.name}");
            Destroy(go);
            return false;
        }

        _weapons.Add(weapon);
        Debug.Log($"[WeaponManager] 무기 추가: {weaponPrefab.name} ({_weapons.Count}/{maxWeaponSlots})");
        return true;
    }

    public bool HasWeapon(GameObject prefab)
    {
        string prefabName = prefab.name + "(Clone)";
        return _weapons.Exists(w => w.gameObject.name == prefabName);
    }

    // WeaponData 기준으로 보유 여부 확인 (LevelUpUI에서 사용)
    public bool HasWeaponByData(WeaponData data)
    {
        return _weapons.Exists(w => w.WeaponData == data);
    }

    // 보유 무기 슬롯 수 확인 (UI 표시용)
    public int WeaponCount => _weapons.Count;
    public bool IsFull => _weapons.Count >= maxWeaponSlots;
}