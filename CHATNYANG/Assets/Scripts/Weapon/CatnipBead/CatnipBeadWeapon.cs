using UnityEngine;

public class CatnipBeadWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        WeaponStatValues stats = weaponData.GetStats(WeaponRarity.Normal, 0);
        if (stats == null) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;

        for (int i = 0; i < stats.projectileCount; i++)
        {
            GameObject bead = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            CatnipBeadProjectile projectileScript = bead.GetComponent<CatnipBeadProjectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, weaponData);
            }
        }
    }
}