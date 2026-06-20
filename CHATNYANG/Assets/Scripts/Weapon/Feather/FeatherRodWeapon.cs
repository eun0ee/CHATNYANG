using UnityEngine;
using System.Collections;

public class FeatherRodWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.areaPrefab == null) return;

        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats == null) return;

        StartCoroutine(SwingRoutine(stats.projectileCount));
    }

    private IEnumerator SwingRoutine(int count)
    {
        Transform target = FindNearestEnemy();
        Vector2 direction = Vector2.up;

        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

        float spawnOffset = 0.6f;
        Vector3 spawnPos = transform.position + (Vector3)(direction * spawnOffset);

        for (int i = 0; i < count; i++)
        {
            GameObject slash = Instantiate(weaponData.areaPrefab, spawnPos, rotation, transform);
            FeatherRodSlash slashScript = slash.GetComponent<FeatherRodSlash>();

            if (slashScript != null)
            {
                slashScript.Initialize(weaponData, transform.position, currentRarity, currentUpgradeLevel);
            }

            yield return new WaitForSeconds(0.05f);
        }
    }
}