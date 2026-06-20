using UnityEngine;

public class BentoniteSandWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        // 무기의 현재 등급과 레벨 상태를 가져옵니다.
        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats == null) return;

        Transform target = FindNearestEnemy();

        Vector2 targetPos = target != null ? (Vector2)target.position : (Vector2)transform.position + Random.insideUnitCircle * 5f;

        for (int i = 0; i < stats.projectileCount; i++)
        {
            GameObject sandBag = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            BentoniteProjectile projectileScript = sandBag.GetComponent<BentoniteProjectile>();

            if (projectileScript != null)
            {
                // 투사체에도 현재 등급과 레벨을 전달합니다.
                projectileScript.Initialize(targetPos, weaponData, currentRarity, currentUpgradeLevel);
            }
        }
    }
}