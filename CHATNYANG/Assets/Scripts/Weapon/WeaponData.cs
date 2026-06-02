using System.Collections.Generic;
using UnityEngine;

// 무기의 희귀도 등급 열거형
public enum WeaponRarity
{
    Normal,
    Rare,
    Epic,
    Unique,
    Legendary
}

// 등급 및 강화별로 변동될 스탯 데이터 구조
[System.Serializable]
public class WeaponStatValues
{
    // 기본 데미지
    public float damage;

    // 공격 쿨타임
    public float attackCooldown;

    // 투사체 발사 개수 (성장 시 증가)
    public int projectileCount = 1;

    // 투사체 바운스 횟수 (성장 시 증가)
    public int bounceCount = 0;

    // 투사체 속도
    public float projectileSpeed = 10f;

    // 벤토나이트 모래 전용 변수: 폭발 및 장판 범위
    public float aoeRadius = 2f;

    // 이속 감소 비율 (0.5 = 50% 속도)
    public float slowFactor = 0.5f;

    // 장판 유지 시간
    public float areaDuration = 4f;
}

// 등급 하나가 가질 강화 데이터 (0~3강)
[System.Serializable]
public class RarityTierData
{
    public WeaponRarity rarity;

    // 0강, 1강, 2강, 3강 스탯 배열 (인스펙터에서 Size를 4로 맞춤)
    public WeaponStatValues[] upgradeLevels = new WeaponStatValues[4];
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "GameData/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Common Weapon Info")]
    // 무기 식별 아이디
    public string weaponId;

    // 투사체 프리팹
    public GameObject projectilePrefab;

    // 터진 후 바닥에 남을 장판 프리팹
    public GameObject areaPrefab;

    [Header("Growth Data by Rarity")]
    // 인스펙터에서 Normal, Rare, Epic 등을 추가하여 스탯을 기입하는 리스트
    public List<RarityTierData> rarityTiers = new List<RarityTierData>();

    // 외부에서 특정 등급과 강화 수치의 스탯을 뽑아오는 함수
    public WeaponStatValues GetStats(WeaponRarity rarity, int upgradeLevel)
    {
        foreach (var tier in rarityTiers)
        {
            if (tier.rarity == rarity)
            {
                // 0강~3강 범위를 벗어나지 않도록 클램프 처리
                int index = Mathf.Clamp(upgradeLevel, 0, 3);
                return tier.upgradeLevels[index];
            }
        }
        return null;
    }
}