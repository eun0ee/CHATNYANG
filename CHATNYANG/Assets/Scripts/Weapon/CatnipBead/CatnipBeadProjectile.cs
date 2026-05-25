using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CatnipBeadProjectile : MonoBehaviour
{
    private float damage;
    private int maxBounce;
    private int currentBounce;
    private Rigidbody2D rb;

    // 투사체 최대 생존 시간
    private float lifeTime = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, WeaponData data)
    {
        damage = data.damage;
        maxBounce = data.bounceCount;
        currentBounce = 0;

        rb.velocity = direction * data.projectileSpeed;

        // 발사 후 lifeTime 초가 지나면 무조건 파괴하여 무한 비행 방지
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 적 타격 처리
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.gameObject.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // 바운스 처리 로직
        if (currentBounce < maxBounce)
        {
            currentBounce++;
            Vector2 reflectDir = Vector2.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
            rb.velocity = reflectDir * rb.velocity.magnitude;
        }
        else
        {
            // 최대 바운스 횟수 도달 시 파괴
            Destroy(gameObject);
        }
    }
}