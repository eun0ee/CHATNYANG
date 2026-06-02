using UnityEngine;

public class BentoniteProjectile : MonoBehaviour
{
    private Vector2 targetPosition;
    private WeaponData data;
    private bool isInitialized = false;

    private float aoeRadius;
    private float speed;
    private float damage;

    // 확장성을 위해 rarity와 upgradeLevel 매개변수 추가
    public void Initialize(Vector2 target, WeaponData weaponData, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        targetPosition = target;
        data = weaponData;

        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[BentoniteProjectile] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            aoeRadius = stats.aoeRadius;
            speed = stats.projectileSpeed;
            damage = stats.damage;
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log($"[Explode] Position: {transform.position}, Radius: {aoeRadius}");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        Debug.Log($"[Explode] Detected Collider Count: {hitEnemies.Length}");

        foreach (Collider2D hit in hitEnemies)
        {
            Debug.Log($"[Explode] Detected: {hit.name} / tag: {hit.tag}");

            if (hit.CompareTag("Enemy"))
            {
                EnemyStats enemy = hit.GetComponent<EnemyStats>();
                Debug.Log($"[Explode] EnemyStats: {(enemy != null ? "Exist" : "Null")}");

                if (enemy != null)
                    enemy.TakeDamage(damage);
            }
        }

        if (data.areaPrefab != null)
        {
            GameObject puddle = Instantiate(data.areaPrefab, transform.position, Quaternion.identity);
            puddle.GetComponent<BentoniteArea>()?.Initialize(data);
        }

        Destroy(gameObject);
    }
}