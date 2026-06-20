using UnityEngine;

public class CatnipDiffuserWeapon : WeaponBase
{
    protected override void ExecuteAttack()
    {
        WeaponStatValues stats = weaponData.GetStats(currentRarity, currentUpgradeLevel);
        if (stats == null) return;

        // 시각적 아우라 생성 및 유지시간 설정
        if (weaponData.areaPrefab != null)
        {
            GameObject aura = Instantiate(weaponData.areaPrefab, transform.position, Quaternion.identity, transform);

            float effectScale = stats.aoeRadius * 2f;
            aura.transform.localScale = new Vector3(effectScale, effectScale, 1f);

            // 하드코딩된 0.5초 대신 기획된 areaDuration을 사용합니다.
            float destroyTime = stats.areaDuration > 0f ? stats.areaDuration : 2f;
            Destroy(aura, destroyTime);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, stats.aoeRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStats enemyStats = col.GetComponent<EnemyStats>();
                if (enemyStats != null)
                {
                    enemyStats.TakeDamage(stats.damage);

                    ConfusionStatus confusion = col.GetComponent<ConfusionStatus>();
                    if (confusion == null)
                    {
                        confusion = col.gameObject.AddComponent<ConfusionStatus>();
                    }

                    // 혼란 유지 시간도 아우라 유지 시간과 동일하게 맞춰줍니다.
                    float confuseTime = stats.areaDuration > 0f ? stats.areaDuration : 2f;
                    confusion.ActivateStatus(confuseTime, enemyStats.Data.moveSpeed);
                }
            }
        }
    }
}