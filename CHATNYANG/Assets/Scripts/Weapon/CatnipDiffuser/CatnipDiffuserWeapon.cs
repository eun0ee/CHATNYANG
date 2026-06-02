using UnityEngine;

public class CatnipDiffuserWeapon : WeaponBase
{
    [Header("Confusion Settings")]
    [SerializeField] private float confusionDuration = 2f;

    protected override void ExecuteAttack()
    {
        WeaponStatValues stats = weaponData.GetStats(WeaponRarity.Normal, 0);
        if (stats == null) return;

        if (weaponData.areaPrefab != null)
        {
            GameObject aura = Instantiate(weaponData.areaPrefab, transform.position, Quaternion.identity, transform);

            float effectScale = stats.aoeRadius * 2f;
            aura.transform.localScale = new Vector3(effectScale, effectScale, 1f);

            Destroy(aura, 0.5f);
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

                    confusion.ActivateStatus(confusionDuration, enemyStats.Data.moveSpeed);
                }
            }
        }
    }
}