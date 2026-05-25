using UnityEngine;

public class CatnipBeadWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;

        // 발사체 생성 및 초기화
        for (int i = 0; i < weaponData.projectileCount; i++)
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