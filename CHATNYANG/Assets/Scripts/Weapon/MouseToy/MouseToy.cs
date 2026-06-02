using UnityEngine;

public class MouseToy : MonoBehaviour
{
    private WeaponData data;
    private Transform targetTransform;

    private float speed;
    private float explosionRadius;
    private float damage;
    private float areaDuration;

    [Header("Zigzag Settings")]
    [SerializeField] private float zigzagFrequency = 15f;
    [SerializeField] private float zigzagMagnitude = 4f;

    private float aliveTimer = 0f;

    public void Initialize(WeaponData weaponData, Transform target, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        data = weaponData;
        targetTransform = target;

        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[MouseToy] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            speed = stats.projectileSpeed;
            explosionRadius = stats.aoeRadius;
            damage = stats.damage;
            areaDuration = stats.areaDuration;
        }

        Destroy(gameObject, 7f);
    }

    private void Update()
    {
        aliveTimer += Time.deltaTime;

        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
        {
            targetTransform = FindHighestHpEnemy();
            if (targetTransform == null)
            {
                Explode();
                return;
            }
        }

        Vector2 baseDirection = ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
        Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);

        float zigzagOffset = Mathf.Sin(aliveTimer * zigzagFrequency) * zigzagMagnitude;
        Vector2 finalMovement = (baseDirection * speed) + (perpendicular * zigzagOffset);

        transform.Translate(finalMovement * Time.deltaTime, Space.World);

        if (finalMovement != Vector2.zero)
        {
            float angle = Mathf.Atan2(finalMovement.y, finalMovement.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (Vector2.Distance(transform.position, targetTransform.position) < 0.2f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStats enemy = col.GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        if (data.areaPrefab != null)
        {
            GameObject effect = Instantiate(data.areaPrefab, transform.position, Quaternion.identity);

            float destroyTime = areaDuration > 0f ? areaDuration : 0.2f;
            Destroy(effect, destroyTime);
        }

        Destroy(gameObject);
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
        return highestHpEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}