using UnityEngine;
using System.Collections;

public class CatnipBeadWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        if (weaponData.projectilePrefab == null) return;

        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats == null) return;

        // 코루틴을 실행하여 시간 간격을 두고 발사합니다.
        StartCoroutine(FireBeadsRoutine(stats));
    }

    private IEnumerator FireBeadsRoutine(WeaponStatValues stats)
    {
        for (int i = 0; i < stats.projectileCount; i++)
        {
            // 매 발사마다 가장 가까운 적을 갱신 (먼저 쏜 구슬에 적이 죽었을 수 있으므로)
            Transform target = FindNearestEnemy();
            if (target == null) yield break;

            Vector2 direction = (target.position - transform.position).normalized;

            GameObject bead = Instantiate(weaponData.projectilePrefab, transform.position, Quaternion.identity);
            CatnipBeadProjectile projectileScript = bead.GetComponent<CatnipBeadProjectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, weaponData, currentRarity, currentUpgradeLevel);
            }

            // 0.15초 대기 후 다음 구슬 발사 (원하는 대로 수치 조절 가능)
            yield return new WaitForSeconds(0.15f);
        }
    }
}