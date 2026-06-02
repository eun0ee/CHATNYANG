using UnityEngine;

public class MouseToyWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        Transform target = FindHighestHpEnemy();

        GameObject mouse = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
        MouseToy mouseScript = mouse.GetComponent<MouseToy>();

        if (mouseScript != null)
        {
            mouseScript.Initialize(weaponData, target);
        }
    }

    private Transform FindHighestHpEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform highestHpEnemy = null;
        float maxHp = -1f;

        foreach (GameObject enemy in enemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats != null && stats.CurrentHp > maxHp)
            {
                maxHp = stats.CurrentHp;
                highestHpEnemy = enemy.transform;
            }
        }

        if (highestHpEnemy == null)
        {
            highestHpEnemy = FindNearestEnemy();
        }

        return highestHpEnemy;
    }
}