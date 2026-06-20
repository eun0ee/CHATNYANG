using UnityEngine;

public class FurBrushWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats == null) return;

        float angleStep = 360f / stats.projectileCount;

        for (int i = 0; i < stats.projectileCount; i++)
        {
            float currentAngle = i * angleStep;
            float radian = currentAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;

            GameObject fur = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);

            FurProjectile projectileScript = fur.GetComponent<FurProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, weaponData, currentRarity, currentUpgradeLevel);
            }
        }
    }
}