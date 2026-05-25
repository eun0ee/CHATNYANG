using UnityEngine;

public enum UpgradeType
{
    WeaponDamage,
    WeaponCooldown,
    WeaponCount,
    WeaponPierce,
    PlayerMaxHp,
    PlayerMoveSpeed,
    PlayerArmor,
    PlayerRecovery,
    NewWeapon,
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "GameData/UpgradeOption")]
public class UpgradeOptionData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;
    public UpgradeType upgradeType;

    [Header("Weapon Upgrade (무기 강화일 때)")]
    public WeaponData targetWeapon;    // 어떤 무기를 강화할지
    public float floatValue;           // 데미지 증가량, 쿨다운 비율 등
    public int intValue;               // 발사 수, 관통 수 증가량

    [Header("New Weapon (신규 무기일 때)")]
    public GameObject weaponPrefab;
}