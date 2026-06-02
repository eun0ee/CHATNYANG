using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CatnipBeadProjectile : MonoBehaviour
{
    private float damage;
    private int maxBounce;
    private int currentBounce;
    private Rigidbody2D rb;

    private float lifeTime = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 확장성을 위해 매개변수 추가
    public void Initialize(Vector2 direction, WeaponData data, WeaponRarity rarity = WeaponRarity.Normal, int upgradeLevel = 0)
    {
        WeaponStatValues stats = data.GetStats(rarity, upgradeLevel);

        if (stats == null)
        {
            Debug.LogWarning("[CatnipBeadProjectile] Stats not found. Using Normal 0 stats.");
            stats = data.GetStats(WeaponRarity.Normal, 0);
        }

        if (stats != null)
        {
            damage = stats.damage;
            maxBounce = stats.bounceCount;
            rb.velocity = direction * stats.projectileSpeed;
        }

        currentBounce = 0;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.gameObject.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        if (currentBounce < maxBounce)
        {
            currentBounce++;
            Vector2 reflectDir = Vector2.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
            rb.velocity = reflectDir * rb.velocity.magnitude;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}