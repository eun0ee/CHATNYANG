using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CatnipBeadProjectile : MonoBehaviour
{
    private float damage;
    private int maxBounce;
    private int currentBounce;
    private Rigidbody2D rb;

    private float lifeTime = 10f;

    // 원래 속도와 직전 프레임의 속도를 기억할 변수
    private float moveSpeed;
    private Vector2 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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
            moveSpeed = stats.projectileSpeed;
            rb.velocity = direction * moveSpeed;
        }

        currentBounce = 0;
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        // 충돌 직전의 속도와 방향을 계속 기록
        lastVelocity = rb.velocity;
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

            // 직전 프레임의 방향(lastVelocity)을 기준으로 반사각 계산
            Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);

            // 원래 속도(moveSpeed)를 강제로 다시 곱해줌으로써 속도 감소 방지
            rb.velocity = reflectDir * Mathf.Max(lastVelocity.magnitude, moveSpeed);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}