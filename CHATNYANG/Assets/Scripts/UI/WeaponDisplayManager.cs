using System.Collections.Generic;
using UnityEngine;

public class WeaponDisplayManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private WeaponSlotUI[] uiSlots; // 미리 배치한 6개의 슬롯

    [Header("Rarity Auras")]
    public Sprite normalAura;
    public Sprite rareAura;
    public Sprite epicAura;
    public Sprite uniqueAura;
    public Sprite legendaryAura;

    private void Start()
    {
        RefreshUI();
    }

    // 무기가 추가되거나 강화될 때마다 호출해야 함
    public void RefreshUI()
    {
        if (weaponManager == null) return;

        IReadOnlyList<WeaponBase> currentWeapons = weaponManager.Weapons;

        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (i < currentWeapons.Count)
            {
                WeaponBase weapon = currentWeapons[i];
                Sprite aura = GetAuraByRarity(weapon.currentRarity);

                // WeaponData에 저장된 아이콘과 아우라, 현재 레벨을 전달
                uiSlots[i].UpdateSlot(weapon.WeaponData.weaponIcon, aura, weapon.currentUpgradeLevel);
            }
            else
            {
                uiSlots[i].ClearSlot();
            }
        }
    }

    private Sprite GetAuraByRarity(WeaponRarity rarity)
    {
        switch (rarity)
        {
            case WeaponRarity.Normal: return normalAura;
            case WeaponRarity.Rare: return rareAura;
            case WeaponRarity.Epic: return epicAura;
            case WeaponRarity.Unique: return uniqueAura;
            case WeaponRarity.Legendary: return legendaryAura;
            default: return normalAura;
        }
    }
}