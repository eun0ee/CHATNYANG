using UnityEngine;

public class CatnipDiffuserWeapon : WeaponBase
{
    [Header("Confusion Settings")]
    [SerializeField] private float confusionDuration = 2f;

    protected override void ExecuteAttack()
    {
        if (weaponData.areaPrefab != null)
        {
            GameObject aura = Instantiate(weaponData.areaPrefab, transform.position, Quaternion.identity, transform);

            // 프리팹의 크기를 데이터의 Aoe Radius에 맞춰서 키워줌 (지름이므로 2배 곱하기)
            float effectScale = weaponData.aoeRadius * 2f;
            aura.transform.localScale = new Vector3(effectScale, effectScale, 1f);

            Destroy(aura, 0.5f);
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weaponData.aoeRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStats stats = col.GetComponent<EnemyStats>();
                if (stats != null)
                {
                    stats.TakeDamage(weaponData.damage);

                    ConfusionStatus confusion = col.GetComponent<ConfusionStatus>();
                    if (confusion == null)
                    {
                        confusion = col.gameObject.AddComponent<ConfusionStatus>();
                    }

                    confusion.ActivateStatus(confusionDuration, stats.Data.moveSpeed);
                }
            }
        }
    }
}