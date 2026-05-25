using UnityEngine;

public class BentoniteSandWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        Transform target = FindNearestEnemy();

        // 적이 없으면 랜덤한 위치로 던짐
        Vector2 targetPos = target != null ? (Vector2)target.position : (Vector2)transform.position + Random.insideUnitCircle * 5f;

        for (int i = 0; i < weaponData.projectileCount; i++)
        {
            GameObject sandBag = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            BentoniteProjectile projectileScript = sandBag.GetComponent<BentoniteProjectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(targetPos, weaponData);
            }
        }
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }
}