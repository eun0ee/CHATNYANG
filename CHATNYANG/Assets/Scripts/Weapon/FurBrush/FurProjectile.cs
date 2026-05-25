using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FurProjectile : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;

    // 관통형이므로 화면 끝까지 뚫고 가도록 수명을 조금 늘립니다.
    private float lifeTime = 4f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, WeaponData data)
    {
        damage = data.damage;

        rb.velocity = direction * data.projectileSpeed;

        // Atan2는 오른쪽(Right)을 0도로 계산합니다. 
        // 원본 스프라이트가 위(Up)를 보고 있으므로 90도를 빼서 방향을 맞춥니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyStats enemy = collision.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 관통을 위해 적과 충돌해도 Destroy(gameObject)를 호출하지 않습니다.
        }
    }
}