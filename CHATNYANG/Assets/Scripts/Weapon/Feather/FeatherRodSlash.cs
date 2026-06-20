using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FeatherRodSlash : MonoBehaviour
{
    private float damage;
    private float knockbackPower = 2.5f;
    private Vector2 sourcePosition;

    public void Initialize(WeaponData data, Vector2 sourcePos, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        sourcePosition = sourcePos;

        // 수정됨: 무조건 Normal, 0을 가져오던 로직 제거. 전달받은 rarity와 upgradeLevel을 확실하게 사용!
        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[FeatherRodSlash] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            damage = stats.damage;
            transform.localScale = Vector3.one * stats.aoeRadius;
        }

        Destroy(gameObject, 0.15f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                // 1. 데미지 적용
                enemy.TakeDamage(damage);

                // 2. 안전한 넉백 로직 (EnemyAI 자체 함수 호출)
                EnemyAI enemyAI = other.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    Vector2 knockbackDir = ((Vector2)other.transform.position - sourcePosition).normalized;
                    // 적을 0.15초 동안 넉백시킵니다.
                    enemyAI.ApplyKnockback(knockbackDir * knockbackPower, 0.15f);
                }
            }
        }
    }
}