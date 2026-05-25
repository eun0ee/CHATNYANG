using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "GameData/WeaponData")]
public class WeaponData : ScriptableObject
{
    // 무기 식별 아이디
    public string weaponId;

    // 기본 데미지
    public float damage;

    // 공격 쿨타임
    public float attackCooldown;

    // 투사체 프리팹
    public GameObject projectilePrefab;

    // 투사체 발사 개수 (성장 시 증가)
    public int projectileCount = 1;

    // 투사체 바운스 횟수 (성장 시 증가)
    public int bounceCount = 0;

    // 투사체 속도
    public float projectileSpeed = 10f;

    // 벤토나이트 모래 전용 추가 변수
    public GameObject areaPrefab; // 터진 후 바닥에 남을 장판 프리팹
    public float aoeRadius = 2f; // 폭발 및 장판 범위
    public float slowFactor = 0.5f; // 이속 감소 비율 (0.5 = 50% 속도)
    public float areaDuration = 4f; // 장판 유지 시간
}