using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData WeaponData => weaponData;
    [SerializeField] protected WeaponData weaponData;

    [Header("Current Weapon State")]
    public WeaponRarity currentRarity = WeaponRarity.Normal;
    public int currentUpgradeLevel = 0;

    protected float currentCooldown;

    // 가챠 결과에 따라 외부에서 언제든 등급과 레벨을 갱신할 수 있는 함수
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

            WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);

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
                currentCooldown = 1f;
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

    protected abstract void ExecuteAttack();
}