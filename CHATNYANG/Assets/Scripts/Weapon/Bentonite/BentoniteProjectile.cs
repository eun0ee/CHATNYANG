using UnityEngine;

public class BentoniteProjectile : MonoBehaviour
{
    private Vector2 targetPosition;
    private WeaponData data;
    private bool isInitialized = false;

    private float aoeRadius;
    private float speed;
    private float damage;

    // 장판에게 넘겨주기 위해 투사체 본인이 등급과 레벨을 기억할 변수 추가
    private WeaponRarity currentRarity;
    private int currentUpgradeLevel;

    public void Initialize(Vector2 target, WeaponData weaponData, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        targetPosition = target;
        data = weaponData;

        // 전달받은 등급과 레벨을 저장해둡니다.
        currentRarity = rarity;
        currentUpgradeLevel = upgradeLevel;

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

            // 장판을 초기화할 때, 아까 기억해둔 등급과 레벨을 함께 넘겨줍니다.
            puddle.GetComponent<BentoniteArea>()?.Initialize(data, currentRarity, currentUpgradeLevel);
        }

        Destroy(gameObject);
    }
}