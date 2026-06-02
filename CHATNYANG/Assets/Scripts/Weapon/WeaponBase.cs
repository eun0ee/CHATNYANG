using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData WeaponData => weaponData;
    [SerializeField] protected WeaponData weaponData;

    [Header("Current Weapon State")]
    public WeaponRarity currentRarity = WeaponRarity.Normal;
    public int currentUpgradeLevel = 0;

    protected float currentCooldown;

    // 외부(WeaponManager 등)에서 무기를 생성할 때 등급과 레벨을 주입해주는 함수
    public virtual void InitializeWeapon(WeaponRarity rarity, int upgradeLevel)
    {
        currentRarity = rarity;
        currentUpgradeLevel = upgradeLevel;

        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats != null)
        {
            currentCooldown = stats.attackCooldown;
        }
    }

    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            ExecuteAttack();

            // 바뀐 데이터 구조에 맞춰 현재 등급의 쿨타임을 다시 가져옴
            WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);

            // 안전장치: 스탯을 못 찾으면 노말 0강 스탯으로 대체
            if (stats == null)
            {
                stats = weaponData.GetStats(WeaponRarity.Normal, 0);
            }

            if (stats != null)
            {
                currentCooldown = stats.attackCooldown;
            }
            else
            {
                currentCooldown = 1f; // 최후의 예외 처리 방어
            }
        }
    }

    protected Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    // 개별 무기들의 공격 로직
    protected abstract void ExecuteAttack();
}