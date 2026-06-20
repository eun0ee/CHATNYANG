using UnityEngine;

public class LaserPointerWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats == null) return;

        for (int i = 0; i < stats.projectileCount; i++)
        {
            GameObject laser = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);

            LaserDot projectileScript = laser.GetComponent<LaserDot>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(transform, weaponData, currentRarity, currentUpgradeLevel);
            }
        }
    }
}